#!/bin/bash
set -e

BOOTSTRAP_SERVERS=${KAFKA_BOOTSTRAP_SERVERS:-"kafka-broker-1:9092,kafka-broker-2:9092,kafka-broker-3:9092"}
MONITOR_INTERVAL=${MONITOR_INTERVAL:-30}
CLUSTER_NAME=${CLUSTER_NAME:-"kraft-cluster-1"}
LOG_DIR="/tmp/kafka-monitor"
LOG_FILE="$LOG_DIR/cluster-monitor.log"

# Create log directory
mkdir -p "$LOG_DIR"
echo "📁 Created log directory: $LOG_DIR"

# Test log file creation
if touch "$LOG_FILE" 2>/dev/null; then
  echo "✅ Log file is writable: $LOG_FILE"
else
  echo "⚠️  Warning: Cannot create log file at $LOG_FILE, will log to console only"
  LOG_FILE="/dev/null"
fi

echo "🎯 Kafka 3-Broker Cluster Monitor Starting..."
echo "Cluster: $CLUSTER_NAME"
echo "Bootstrap servers: $BOOTSTRAP_SERVERS"
echo "Monitor interval: ${MONITOR_INTERVAL}s"
echo "Log file: $LOG_FILE"

log_with_timestamp() {
  local message="$(date '+%Y-%m-%d %H:%M:%S') - $1"
  echo "$message"
  
  # Try to write to log file, fallback to console only if failed
  if ! echo "$message" >> "$LOG_FILE" 2>/dev/null; then
    # If log file write fails, just output to console
    echo "⚠️  Warning: Cannot write to log file $LOG_FILE"
  fi
}

check_cluster_health() {
  log_with_timestamp "🔍 Checking 3-broker cluster health..."
  
  # Test basic connectivity first
  if ! kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --list >/dev/null 2>&1; then
    log_with_timestamp "❌ Cannot connect to Kafka cluster at $BOOTSTRAP_SERVERS"
    return 1
  fi
  
  log_with_timestamp "✅ Successfully connected to Kafka cluster"
  
  # Check individual broker health and collect names
  local healthy_brokers=()
  local unhealthy_brokers=()
  
  for broker in "kafka-broker-1:19092" "kafka-broker-2:19092" "kafka-broker-3:19092"; do
    local broker_name=$(echo "$broker" | cut -d':' -f1)
    if kafka-topics --bootstrap-server "$broker" --list >/dev/null 2>&1; then
      healthy_brokers+=("$broker_name")
    else
      unhealthy_brokers+=("$broker_name")
    fi
  done
  
  local broker_count=${#healthy_brokers[@]}
  log_with_timestamp "========================== BROKER HEALTH CHECK ============================"
  log_with_timestamp "📊 Active brokers: $broker_count/3"
  
  if [ $broker_count -gt 0 ]; then
    local healthy_list=$(IFS=', '; echo "${healthy_brokers[*]}")
    log_with_timestamp "  ✅ Healthy: [$healthy_list]"
  fi
  
  if [ ${#unhealthy_brokers[@]} -gt 0 ]; then
    local unhealthy_list=$(IFS=', '; echo "${unhealthy_brokers[*]}")
    log_with_timestamp "  ❌ Unhealthy: [$unhealthy_list]"
  fi
  
  if [ $broker_count -lt 3 ]; then
    log_with_timestamp "⚠️  WARNING: Only $broker_count/3 brokers available!"
  else
    log_with_timestamp "✅ All 3 brokers are healthy"
  fi
  
  # Check topics with better error handling
  local topics_output=$(kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --list 2>/dev/null || echo "")
  local topics=0
  
  if [ -n "$topics_output" ]; then
    topics=$(echo "$topics_output" | grep -v "^__" | grep -v "^$" | wc -l)
  fi
  
  log_with_timestamp "📋 Available topics: $topics"
  
  # List actual topics for debugging
  if [ $topics -gt 0 ]; then
    log_with_timestamp "📝 Topic list:"
    echo "$topics_output" | grep -v "^__" | grep -v "^$" | while read topic; do
      if [ -n "$topic" ]; then
        log_with_timestamp "  └── $topic"
      fi
    done
  fi
  
  # Check under-replicated partitions with better error handling
  local describe_output=$(kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --describe 2>/dev/null || echo "")
  local under_replicated=0
  
  if [ -n "$describe_output" ]; then
    under_replicated=$(echo "$describe_output" | grep -c "UnderReplicated" || echo 0)
  fi
  
  if [ $under_replicated -gt 0 ]; then
    log_with_timestamp "⚠️  WARNING: $under_replicated under-replicated partitions"
    
    # Log details of under-replicated partitions
    echo "$describe_output" | grep "UnderReplicated" | while read line; do
      if [ -n "$line" ]; then
        log_with_timestamp "  └── $line"
      fi
    done
  else
    log_with_timestamp "✅ All partitions properly replicated"
  fi
  
  # Check offline partitions
  local offline_partitions=0
  if [ -n "$describe_output" ]; then
    offline_partitions=$(echo "$describe_output" | grep -c "OfflineReplicas" || echo 0)
  fi
  
  if [ $offline_partitions -gt 0 ]; then
    log_with_timestamp "❌ CRITICAL: $offline_partitions offline partitions!"
    return 1
  fi
  
  # Check leader distribution
  check_leader_balance
  
  log_with_timestamp "✅ Cluster health check completed"
  return 0
}

check_leader_balance() {
  log_with_timestamp "⚖️  Checking leader balance across 3 brokers..."
  
  local describe_output=$(kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --describe 2>/dev/null || echo "")
  
  if [ -z "$describe_output" ]; then
    log_with_timestamp "⚠️  Cannot get topic descriptions for leader balance check"
    return 1
  fi
  
  declare -A leader_counts
  # Initialize counters
  leader_counts[1]=0
  leader_counts[2]=0
  leader_counts[3]=0
  
  # Count leaders for each broker
  for broker_id in 1 2 3; do
    local count=$(echo "$describe_output" | grep "Leader: $broker_id" | wc -l || echo 0)
    leader_counts[$broker_id]=$count
    log_with_timestamp "  Broker-$broker_id: $count leaders"
  done
  
  # Calculate balance without bc (not available in all containers)
  local max_leaders=0
  local min_leaders=999999
  local total_leaders=0
  
  for broker_id in 1 2 3; do
    local count=${leader_counts[$broker_id]}
    total_leaders=$((total_leaders + count))
    
    if [ $count -gt $max_leaders ]; then 
      max_leaders=$count
    fi
    
    if [ $count -lt $min_leaders ] && [ $count -ge 0 ]; then 
      min_leaders=$count
    fi
  done
  
  log_with_timestamp "  Total leaders across cluster: $total_leaders"
  log_with_timestamp "  Leader range: $min_leaders to $max_leaders"
  
  if [ $max_leaders -gt 0 ] && [ $min_leaders -lt 999999 ]; then
    local diff=$((max_leaders - min_leaders))
    if [ $diff -le 1 ]; then
      log_with_timestamp "  ✅ Well-balanced cluster (difference: $diff)"
    else
      log_with_timestamp "  ⚠️  Consider rebalancing cluster (difference: $diff)"
    fi
  else
    log_with_timestamp "  ℹ️  Cannot calculate balance (no leaders found)"
  fi
}

# Function to monitor detailed broker information
monitor_broker_details() {
  log_with_timestamp "🔍 Analyzing individual broker details..."
  
  local describe_output=$(kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --describe 2>/dev/null || echo "")
  
  if [ -z "$describe_output" ]; then
    log_with_timestamp "⚠️  Cannot get topic descriptions for broker analysis"
    return 1
  fi
  log_with_timestamp "========================== BROKER DETAILS ============================"
  for broker_id in 1 2 3; do
    local broker_host="kafka-broker-${broker_id}:19092"
    log_with_timestamp "📊 Analyzing Broker-${broker_id} (${broker_host}):"
    
    # Test broker connectivity
    if ! kafka-topics --bootstrap-server "$broker_host" --list >/dev/null 2>&1; then
      log_with_timestamp "  ❌ Broker-${broker_id} is not accessible"
      continue
    fi
    
    # Get topics list for this broker
    local topics_output=$(kafka-topics --bootstrap-server "$broker_host" --list 2>/dev/null | grep -v "^__" | grep -v "^$" || echo "")
    local topic_count=0
    
    if [ -n "$topics_output" ]; then
      topic_count=$(echo "$topics_output" | wc -l)
    fi
    
    log_with_timestamp "  📋 Total topics accessible: $topic_count"
    
    # Analyze leadership for this broker
    local leadership_info=$(echo "$describe_output" | grep "Leader: $broker_id" || echo "")
    local leader_count=0
    
    if [ -n "$leadership_info" ]; then
      leader_count=$(echo "$leadership_info" | wc -l)
    fi
    
    log_with_timestamp "  👑 Leading partitions: $leader_count"
    
    # Analyze replica participation
    local replica_info=$(echo "$describe_output" | grep "Replicas:.*$broker_id" || echo "")
    local replica_count=0
    
    if [ -n "$replica_info" ]; then
      replica_count=$(echo "$replica_info" | wc -l)
    fi
    
    log_with_timestamp "  🔄 Replica partitions: $replica_count"
    
    # List topics where this broker is leader (max 5 for readability)
    if [ $leader_count -gt 0 ]; then
      log_with_timestamp "  👑 Topics where Broker-${broker_id} is leader:"
      echo "$leadership_info" | head -5 | while read line; do
        if [ -n "$line" ]; then
          local topic_name=$(echo "$line" | awk '{print $2}')
          local partition_num=$(echo "$line" | awk '{print $4}')
          log_with_timestamp "    └── ${topic_name}[${partition_num}]"
        fi
      done
      
      if [ $leader_count -gt 5 ]; then
        log_with_timestamp "    └── ... and $((leader_count - 5)) more partitions"
      fi
    fi
    
    # Check broker-specific health metrics
    check_broker_specific_health "$broker_id" "$describe_output"
    
    log_with_timestamp "  ✅ Broker-${broker_id} analysis completed"
    echo ""
  done
}

# Function to check broker-specific health
check_broker_specific_health() {
  local broker_id=$1
  local describe_output="$2"
  
  log_with_timestamp "  🏥 Health metrics for Broker-${broker_id}:"
  
  if [ -n "$describe_output" ]; then
    # Under-replicated partitions where this broker is leader
    local under_repl_as_leader=$(echo "$describe_output" | grep "Leader: $broker_id" | grep "UnderReplicated" | wc -l || echo 0)
    
    # Under-replicated partitions where this broker is replica
    local under_repl_as_replica=$(echo "$describe_output" | grep "Replicas:.*$broker_id" | grep "UnderReplicated" | wc -l || echo 0)
    
    if [ $under_repl_as_leader -gt 0 ]; then
      log_with_timestamp "    ⚠️  Under-replicated as leader: $under_repl_as_leader partitions"
    else
      log_with_timestamp "    ✅ No under-replicated partitions as leader"
    fi
    
    if [ $under_repl_as_replica -gt 0 ]; then
      log_with_timestamp "    ⚠️  Under-replicated as replica: $under_repl_as_replica partitions"
    else
      log_with_timestamp "    ✅ No under-replicated partitions as replica"
    fi
    
    # Check if broker is in ISR for all its replicas
    local total_replicas=$(echo "$describe_output" | grep "Replicas:.*$broker_id" | wc -l || echo 0)
    local in_isr=$(echo "$describe_output" | grep "Replicas:.*$broker_id" | grep "Isr:.*$broker_id" | wc -l || echo 0)
    local isr_issues=$((total_replicas - in_isr))
    
    if [ $isr_issues -gt 0 ]; then
      log_with_timestamp "    ⚠️  Not in ISR for $isr_issues partitions"
    else
      log_with_timestamp "    ✅ In-sync for all replica partitions"
    fi
    
    # Check offline partitions for this broker
    local offline_partitions=$(echo "$describe_output" | grep "Leader: $broker_id" | grep "OfflineReplicas" | wc -l || echo 0)
    
    if [ $offline_partitions -gt 0 ]; then
      log_with_timestamp "    ❌ CRITICAL: $offline_partitions offline partitions as leader"
    else
      log_with_timestamp "    ✅ No offline partitions"
    fi
  else
    log_with_timestamp "    ⚠️  Cannot retrieve health metrics"
  fi
}

# Function to analyze topic distribution across brokers
analyze_topic_distribution() {
  log_with_timestamp "========================== TOPIC DISTRIBUTION ============================"
  log_with_timestamp "📊 Topic distribution analysis across 3-broker cluster:"
  
  local describe_output=$(kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --describe 2>/dev/null || echo "")
  
  if [ -z "$describe_output" ]; then
    log_with_timestamp "⚠️  Cannot get topic descriptions for distribution analysis"
    return 1
  fi
  
  # Get unique topics
  local topics=$(echo "$describe_output" | grep "PartitionCount" | awk '{print $2}' | sort -u)
  
  if [ -z "$topics" ]; then
    log_with_timestamp "⚠️  No topics found for analysis"
    return 1
  fi
  
  log_with_timestamp "📋 Per-topic analysis:"
  
  echo "$topics" | while read topic; do
    if [ -n "$topic" ]; then
      log_with_timestamp "  📄 Topic: $topic"
      
      # Get topic details
      local topic_detail=$(echo "$describe_output" | grep "Topic: $topic")
      local partition_count=$(echo "$topic_detail" | awk '{print $4}' | head -1)
      local replication_factor=$(echo "$topic_detail" | awk '{print $6}' | head -1)
      
      log_with_timestamp "    ├── Partitions: $partition_count"
      log_with_timestamp "    ├── Replication Factor: $replication_factor"
      
      # Analyze partition distribution for this topic
      declare -A topic_leaders
      topic_leaders[1]=0
      topic_leaders[2]=0  
      topic_leaders[3]=0
      
      local topic_partitions=$(echo "$describe_output" | grep "Topic: $topic" -A 999 | grep "Leader:" | head -n "$partition_count")
      
      for broker_id in 1 2 3; do
        local count=$(echo "$topic_partitions" | grep "Leader: $broker_id" | wc -l || echo 0)
        topic_leaders[$broker_id]=$count
        log_with_timestamp "    ├── Broker-$broker_id leaders: $count"
      done
      
      # Check if topic has any issues
      local topic_under_repl=$(echo "$describe_output" | grep "Topic: $topic" -A 999 | grep "UnderReplicated" | head -n "$partition_count" | wc -l || echo 0)
      
      if [ $topic_under_repl -gt 0 ]; then
        log_with_timestamp "    └── ⚠️  $topic_under_repl under-replicated partitions"
      else
        log_with_timestamp "    └── ✅ All partitions healthy"
      fi
      
      echo ""
    fi
  done
}

monitor_specific_topics() {
  log_with_timestamp "📋 Monitoring key topics..."
  
  local key_topics=("user-events" "order-events" "audit-logs" "helpdesk-tickets" "survey-responses")
  local topics_list=$(kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --list 2>/dev/null || echo "")
  
  if [ -z "$topics_list" ]; then
    log_with_timestamp "⚠️  Cannot retrieve topics list"
    return 1
  fi
  
  for topic in "${key_topics[@]}"; do
    if echo "$topics_list" | grep -q "^${topic}$"; then
      local topic_describe=$(kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --describe --topic "$topic" 2>/dev/null || echo "")
      
      if [ -n "$topic_describe" ]; then
        local partition_count=$(echo "$topic_describe" | grep "PartitionCount" | awk '{print $4}' || echo "unknown")
        local under_repl=$(echo "$topic_describe" | grep -c "UnderReplicated" || echo 0)
        
        if [ $under_repl -eq 0 ]; then
          log_with_timestamp "  ✅ $topic: $partition_count partitions, healthy"
        else
          log_with_timestamp "  ⚠️  $topic: $partition_count partitions, $under_repl under-replicated"
        fi
      else
        log_with_timestamp "  ⚠️  $topic: Cannot get topic details"
      fi
    else
      log_with_timestamp "  ❌ $topic: MISSING!"
    fi
  done
}

# Trap signals for graceful shutdown
trap 'log_with_timestamp "📴 Kafka monitor shutting down..."; exit 0' SIGTERM SIGINT

# Main monitoring loop
log_with_timestamp "🚀 Enhanced 3-broker Kafka monitoring started"
log_with_timestamp "📝 Debug info: Using bootstrap servers: $BOOTSTRAP_SERVERS"
log_with_timestamp "📝 Debug info: Monitor interval: ${MONITOR_INTERVAL}s"
log_with_timestamp "📝 Debug info: Cluster name: $CLUSTER_NAME"
log_with_timestamp "🔍 Features: Broker details, topic distribution, health metrics"

while true; do
  log_with_timestamp "=========================================== BEGIN MONITORING ============================================"
  log_with_timestamp "🔄 Starting comprehensive cluster analysis..."
  
  if check_cluster_health; then
    log_with_timestamp "✅ Basic health check passed"
    
    # Add detailed broker monitoring
    monitor_broker_details
    
    # Add topic distribution analysis  
    analyze_topic_distribution
    
    # Keep existing specific topic monitoring
    monitor_specific_topics
    
    log_with_timestamp "😴 Sleeping for ${MONITOR_INTERVAL} seconds..."
  else
    log_with_timestamp "❌ Cluster health check failed, retrying in ${MONITOR_INTERVAL} seconds..."
  fi
  
  echo "===============================================" >> "$LOG_FILE"
  sleep $MONITOR_INTERVAL
done
