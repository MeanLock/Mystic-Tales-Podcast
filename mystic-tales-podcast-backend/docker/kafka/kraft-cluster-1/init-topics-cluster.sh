#!/bin/bash
set -e

echo "🚀 Starting Kafka topics initialization for 3-broker cluster..."
echo "🐳 Managed via Docker UI - No external shell scripts needed"

BOOTSTRAP_SERVERS=${KAFKA_BOOTSTRAP_SERVERS:-"kafka-broker-1:9092,kafka-broker-2:9092,kafka-broker-3:9092"}
CLUSTER_NAME=${CLUSTER_NAME:-"kraft-cluster-1"}

echo "Cluster: $CLUSTER_NAME"
echo "Bootstrap servers: $BOOTSTRAP_SERVERS"
echo "Topics source: ${TOPICS_CONFIG:+Environment Variable}${TOPICS_CONFIG:-Default Hardcoded}"

# Function to check which brokers are running
check_running_brokers() {
  echo "🔍 Checking broker availability..."
  
  local all_brokers=()
  local running_brokers=()
  local failed_brokers=()
  
  # Parse bootstrap servers
  IFS=',' read -ra BROKER_LIST <<< "$BOOTSTRAP_SERVERS"
  
  for broker in "${BROKER_LIST[@]}"; do
    broker=$(echo "$broker" | xargs)  # Trim whitespace
    all_brokers+=("$broker")
    
    echo "  Testing connection to $broker..."
    if kafka-topics --bootstrap-server "$broker" --list >/dev/null 2>&1; then
      echo "  ✅ $broker is running"
      running_brokers+=("$broker")
    else
      echo "  ❌ $broker is not responding"
      failed_brokers+=("$broker")
    fi
  done
  
  echo ""
  echo "📊 Broker Status Summary:"
  echo "  Total brokers: ${#all_brokers[@]}"
  echo "  Running brokers: ${#running_brokers[@]}"
  echo "  Failed brokers: ${#failed_brokers[@]}"
  
  if [ ${#running_brokers[@]} -eq 0 ]; then
    echo "❌ CRITICAL: No brokers are responding!"
    echo "🚨 Cannot proceed with topic initialization"
    echo "💡 Please check if Kafka brokers are running and accessible"
    exit 1
  fi
  
  if [ ${#failed_brokers[@]} -gt 0 ]; then
    echo "⚠️  WARNING: Some brokers are not responding: ${failed_brokers[*]}"
    echo "📝 Proceeding with available brokers: ${running_brokers[*]}"
  else
    echo "✅ All brokers are healthy and responding"
  fi
  
  # Update BOOTSTRAP_SERVERS to only include running brokers
  BOOTSTRAP_SERVERS=$(IFS=','; echo "${running_brokers[*]}")
  echo "🎯 Using active brokers: $BOOTSTRAP_SERVERS"
  
  # Validate minimum brokers for replication requirements
  local min_brokers_needed=2  # For topics with replication factor 3, need at least 2 brokers
  if [ ${#running_brokers[@]} -lt $min_brokers_needed ]; then
    echo "⚠️  WARNING: Only ${#running_brokers[@]} broker(s) running"
    echo "📝 Some topics with replication factor 3 may not be created properly"
    echo "💡 Recommended: Have at least $min_brokers_needed brokers running"
  fi
  
  echo ""
  
  return 0
}

# Check broker health before proceeding
check_running_brokers

# Load topics from environment variable or use defaults
if [ -n "$TOPICS_CONFIG" ]; then
  echo "📋 Loading topics from TOPICS_CONFIG environment variable..."
  # Convert multiline environment variable to array
  readarray -t TOPICS <<< "$TOPICS_CONFIG"
  # Remove empty lines
  TOPICS=($(printf '%s\n' "${TOPICS[@]}" | grep -v '^[[:space:]]*$'))
else
  echo "📋 Using default topics configuration..."
  # Fallback topics if environment variable is not set
  TOPICS=(
    # Format: "topic-name:partitions:replication-factor:min-isr:retention-hours"
    "user-events:9:3:2:168"              # 9 partitions = 3 per broker
    "order-events:12:3:2:720"            # 12 partitions = 4 per broker
    "notification-events:6:3:2:168"      # 6 partitions = 2 per broker
    "facility-events:9:3:2:720"          # 9 partitions = 3 per broker, 30 days retention
    "audit-logs:3:3:3:8760"             # Critical: all brokers, 1 year retention
    "helpdesk-tickets:9:3:2:2160"       # HelpDesk service - 90 days
    "survey-responses:12:3:2:1440"      # Survey service - 60 days
    "gateway-requests:6:3:2:168"        # API Gateway logs
    "system-health:3:2:1:72"            # Health checks - 3 days
    "security-events:3:3:3:8760"        # Security logs - 1 year
    "performance-metrics:6:2:1:24"      # Performance data - 1 day
  )
fi

echo "📊 Topics to be processed:"
for i in "${!TOPICS[@]}"; do
  topic_line="${TOPICS[$i]}"
  # Skip empty lines
  if [ -n "$topic_line" ]; then
    IFS=":" read -r name parts repl min_isr retention <<< "$topic_line"
    echo "  $((i+1)). $name ($parts partitions, replication: $repl, retention: ${retention}h)"
  fi
done

echo "📋 Creating ${#TOPICS[@]} topics optimized for 3-broker cluster..."

# Function to check if topic exists and has correct configuration
check_topic_config() {
  local name=$1
  local expected_partitions=$2
  local expected_replication=$3
  local expected_min_isr=$4
  local expected_retention_hours=$5
  
  echo "🔍 Checking existing configuration for topic: $name"
  
  # Check if topic exists
  if ! kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --list 2>/dev/null | grep -q "^${name}$"; then
    echo "  ❌ Topic does not exist"
    return 1
  fi
  
  # Get topic description
  local topic_desc=$(kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --describe --topic "$name" 2>/dev/null)
  if [ -z "$topic_desc" ]; then
    echo "  ❌ Cannot get topic description"
    return 1
  fi
  
  # Extract current configuration
  local current_partitions=$(echo "$topic_desc" | grep "PartitionCount:" | awk '{print $2}' | head -1)
  local current_replication=$(echo "$topic_desc" | grep "ReplicationFactor:" | awk '{print $2}' | head -1)
  
  # Get topic configs
  local topic_configs=$(kafka-configs --bootstrap-server $BOOTSTRAP_SERVERS --describe --entity-type topics --entity-name "$name" 2>/dev/null | grep "configs=")
  
  # Extract current min.insync.replicas and retention.ms
  local current_min_isr=$(echo "$topic_configs" | grep -o "min.insync.replicas=[^,]*" | cut -d'=' -f2)
  local current_retention_ms=$(echo "$topic_configs" | grep -o "retention.ms=[^,]*" | cut -d'=' -f2)
  
  # Set defaults if not found
  current_min_isr=${current_min_isr:-1}  # Default min ISR is 1
  current_retention_ms=${current_retention_ms:-604800000}  # Default 7 days
  
  # Convert expected retention to ms
  local expected_retention_ms=$((expected_retention_hours * 3600000))
  
  echo "  📊 Current config:"
  echo "    ├── Partitions: $current_partitions"
  echo "    ├── Replication: $current_replication" 
  echo "    ├── Min ISR: $current_min_isr"
  echo "    └── Retention: $current_retention_ms ms"
  
  echo "  🎯 Expected config:"
  echo "    ├── Partitions: $expected_partitions"
  echo "    ├── Replication: $expected_replication"
  echo "    ├── Min ISR: $expected_min_isr"
  echo "    └── Retention: $expected_retention_ms ms"
  
  # Compare configurations
  if [ "$current_partitions" != "$expected_partitions" ]; then
    echo "  ⚠️  Partition count mismatch: $current_partitions ≠ $expected_partitions"
    return 1
  fi
  
  if [ "$current_replication" != "$expected_replication" ]; then
    echo "  ⚠️  Replication factor mismatch: $current_replication ≠ $expected_replication"
    return 1
  fi
  
  if [ "$current_min_isr" != "$expected_min_isr" ]; then
    echo "  ⚠️  Min ISR mismatch: $current_min_isr ≠ $expected_min_isr"
    return 1
  fi
  
  if [ "$current_retention_ms" != "$expected_retention_ms" ]; then
    echo "  ⚠️  Retention mismatch: $current_retention_ms ≠ $expected_retention_ms"
    return 1
  fi
  
  echo "  ✅ Configuration matches perfectly!"
  return 0
}

create_topic() {
  local name=$1
  local partitions=$2
  local replication=$3
  local min_isr=$4
  local retention_hours=$5
  
  echo "🔄 Processing topic: $name"
  echo "  ├── Partitions: $partitions (${partitions}/3 = $((partitions/3)) per broker)"
  echo "  ├── Replication Factor: $replication"
  echo "  ├── Min In-Sync Replicas: $min_isr"
  echo "  └── Retention: ${retention_hours} hours"
  
  # Validate replication requirements against available brokers
  local running_broker_count=$(echo "$BOOTSTRAP_SERVERS" | tr ',' '\n' | wc -l)
  local adjusted_replication=$replication
  local adjusted_min_isr=$min_isr
  
  if [ $replication -gt $running_broker_count ]; then
    echo "  ⚠️  WARNING: Replication factor ($replication) > running brokers ($running_broker_count)"
    echo "  📝 Adjusting replication factor to $running_broker_count"
    adjusted_replication=$running_broker_count
    
    # Adjust min ISR if necessary
    if [ $min_isr -gt $adjusted_replication ]; then
      adjusted_min_isr=$((adjusted_replication - 1))
      if [ $adjusted_min_isr -lt 1 ]; then
        adjusted_min_isr=1
      fi
      echo "  📝 Adjusting min ISR to $adjusted_min_isr"
    fi
  fi
  
  # Check if topic exists and has correct configuration
  if check_topic_config "$name" "$partitions" "$adjusted_replication" "$adjusted_min_isr" "$retention_hours"; then
    echo "  ✅ Topic already exists with correct configuration - skipping creation"
    echo "  💾 No write operations needed for this topic"
    return 0
  fi
  
  # Topic needs to be created or updated
  echo "  🔧 Topic needs to be created or updated..."
  
  # Check if topic exists but with wrong config - delete it first
  if kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --list 2>/dev/null | grep -q "^${name}$"; then
    echo "  🗑️  Deleting existing topic with incorrect configuration: $name"
    kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --delete --topic "$name" 2>/dev/null || {
      echo "  ⚠️  Failed to delete topic $name, continuing..."
    }
    
    # Wait a bit for deletion to complete
    echo "  ⏳ Waiting for topic deletion to complete..."
    sleep 2
    
    # Verify deletion
    local attempts=0
    while kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --list 2>/dev/null | grep -q "^${name}$" && [ $attempts -lt 10 ]; do
      echo "  ⏳ Still waiting for deletion... (attempt $((attempts + 1))/10)"
      sleep 2
      attempts=$((attempts + 1))
    done
    
    if [ $attempts -eq 10 ]; then
      echo "  ⚠️  Topic deletion timeout, but continuing with creation..."
    else
      echo "  ✅ Topic successfully deleted"
    fi
  else
    echo "  ℹ️  Topic does not exist, creating new..."
  fi
  
  local retention_ms=$((retention_hours * 3600000))
  
  echo "  🆕 Creating topic with new configuration..."
  kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS \
    --create \
    --topic "$name" \
    --partitions "$partitions" \
    --replication-factor "$adjusted_replication" \
    --config min.insync.replicas="$adjusted_min_isr" \
    --config retention.ms="$retention_ms" \
    --config segment.ms=86400000 \
    --config compression.type=lz4 \
    --config cleanup.policy=delete \
    --config unclean.leader.election.enable=false
    
  return $?
}

# Apply topics to all servers with smart configuration checking
echo "🔄 SMART MODE: Checking existing topics and only updating when necessary"
echo "🚀 Processing topics across cluster..."
echo ""

success_count=0
skipped_count=0
updated_count=0
failed_topics=()

for topic_line in "${TOPICS[@]}"; do
  # Skip empty lines
  if [ -z "$topic_line" ]; then
    continue
  fi
  
  IFS=":" read -r name parts repl min_isr retention <<< "$topic_line"
  
  # Skip if topic line is malformed
  if [ -z "$name" ] || [ -z "$parts" ] || [ -z "$repl" ] || [ -z "$min_isr" ] || [ -z "$retention" ]; then
    echo "⚠️  Skipping malformed topic line: $topic_line"
    continue
  fi
  
  topic_result=$(create_topic "$name" "$parts" "$repl" "$min_isr" "$retention" 2>&1)
  exit_status=$?
  
  if [ $exit_status -eq 0 ]; then
    # Check if topic was skipped (already correct) or created/updated
    if echo "$topic_result" | grep -q "skipping creation"; then
      echo "✅ Topic already correct: $name"
      skipped_count=$((skipped_count + 1))
    else
      echo "✅ Successfully created/updated topic: $name"
      updated_count=$((updated_count + 1))
    fi
    success_count=$((success_count + 1))
  else
    echo "❌ Failed to create topic: $name"
    failed_topics+=("$name")
    # Continue with other topics instead of exiting
  fi
  echo ""
done

echo "🎉 Topic processing summary: $success_count/${#TOPICS[@]} topics processed successfully"
echo "  ✅ Already correct (skipped): $skipped_count"
echo "  🔄 Created/Updated: $updated_count"

if [ ${#failed_topics[@]} -gt 0 ]; then
  echo "❌ Failed topics: ${failed_topics[*]}"
  echo "⚠️  Some topics failed, but cluster initialization continued"
else
  echo "✅ All topics processed successfully!"
  if [ $skipped_count -gt 0 ]; then
    echo "💾 $skipped_count topics were already configured correctly - no unnecessary writes performed"
  fi
fi

# Verify cluster topology after smart processing
echo ""
echo "📊 Cluster topology verification (after smart processing):"
echo "=========================================================="

final_topic_list=$(kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --list 2>/dev/null | grep -v "^__" | sort)

if [ -z "$final_topic_list" ]; then
  echo "⚠️  No topics found in cluster"
else
  echo "📋 Final topic list:"
  echo "$final_topic_list" | while read topic; do
    if [ ! -z "$topic" ]; then
      echo "  ✅ $topic"
    fi
  done
  
  echo ""
  echo "🔍 Topic details (first 3 partitions per topic):"
  echo "$final_topic_list" | while read topic; do
    if [ ! -z "$topic" ]; then
      echo ""
      echo "Topic: $topic"
      kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --describe --topic "$topic" 2>/dev/null | \
        grep "Partition" | head -3 | awk '{printf "  P%s: Leader=Broker-%s, Replicas=[%s]\n", $2, $6, $8}' || \
        echo "  ⚠️  Could not get partition details"
    fi
  done
fi

echo ""
if [ ${#failed_topics[@]} -eq 0 ]; then
  echo "✅ 3-broker Kafka cluster initialization completed successfully!"
  if [ $updated_count -gt 0 ]; then
    echo "🎯 $updated_count topics were created/updated with new configurations"
  fi
  if [ $skipped_count -gt 0 ]; then
    echo "💾 $skipped_count topics were already correct - avoided unnecessary broker writes"
  fi
  exit_code=0
else
  echo "⚠️  3-broker Kafka cluster initialization completed with some failures"
  echo "🎯 $updated_count topics created/updated, $skipped_count skipped, check failed topics: ${failed_topics[*]}"
  exit_code=1
fi

# Final summary and cleanup
echo ""
echo "🏁 INITIALIZATION SUMMARY:"
echo "========================="
echo "📊 Topics processed: ${#TOPICS[@]}"
echo "✅ Total successful: $success_count"
echo "  ├── Created/Updated: $updated_count"
echo "  └── Already correct (skipped): $skipped_count"
echo "❌ Failed: ${#failed_topics[@]}"
echo "🔗 Active brokers used: $BOOTSTRAP_SERVERS"
echo "⏰ Completed at: $(date '+%Y-%m-%d %H:%M:%S')"

if [ $exit_code -eq 0 ]; then
  echo ""
  echo "🎉 Kafka cluster is ready for use!"
  echo "💡 You can now start producing and consuming messages"
  if [ $skipped_count -gt 0 ]; then
    echo "🚀 Optimized: $skipped_count topics avoided unnecessary writes to brokers"
  fi
else
  echo ""
  echo "⚠️  Cluster initialization had some issues"
  echo "🔧 Please check the failed topics and broker connectivity"
fi

echo ""
echo "🛑 kafka-init service is now stopping..."
echo "🎯 Service configured to NOT restart automatically (restart: no)"
echo "📋 Use 'docker-compose logs kafka-init' to view this log again"
echo "💡 To re-run: docker-compose up kafka-init"
echo "🐳 Manage via Docker UI: Container will show as 'Exited' when completed"

# Ensure script exits with proper code
echo "🔚 Script terminating with exit code: $exit_code"
exit $exit_code
