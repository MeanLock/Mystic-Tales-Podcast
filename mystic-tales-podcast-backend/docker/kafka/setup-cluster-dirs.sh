#!/bin/bash

# 📁 Kafka Cluster Directory Setup Script
# Script này sẽ tạo cấu trúc thư mục cho các cluster Kafka

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
KAFKA_DIR="$SCRIPT_DIR"

echo "🗂️  Setting up Kafka cluster directory structure..."
echo "Kafka directory: $KAFKA_DIR"

# Function to create cluster directory structure
create_cluster_structure() {
    local cluster_name=$1
    local cluster_dir="$KAFKA_DIR/$cluster_name"
    
    echo "📁 Creating directory structure for cluster: $cluster_name"
    
    # Create cluster directory
    mkdir -p "$cluster_dir"
    echo "✅ Created directory: $cluster_dir"
    
    # Check if we need to move existing files
    if [ "$cluster_name" = "kraft-cluster-1" ]; then
        # Check for loose files in kafka directory
        if [ -f "$KAFKA_DIR/init-topics-cluster.sh" ] && [ ! -f "$cluster_dir/init-topics-cluster.sh" ]; then
            echo "📦 Moving init-topics-cluster.sh to $cluster_name directory..."
            mv "$KAFKA_DIR/init-topics-cluster.sh" "$cluster_dir/"
        fi
        
        if [ -f "$KAFKA_DIR/monitor-cluster.sh" ] && [ ! -f "$cluster_dir/monitor-cluster.sh" ]; then
            echo "📦 Moving monitor-cluster.sh to $cluster_name directory..."
            mv "$KAFKA_DIR/monitor-cluster.sh" "$cluster_dir/"
        fi
    fi
    
    # Create template files if they don't exist
    if [ ! -f "$cluster_dir/init-topics-cluster.sh" ]; then
        echo "📝 Creating template init-topics-cluster.sh for $cluster_name..."
        cat > "$cluster_dir/init-topics-cluster.sh" << 'EOF'
#!/bin/bash
# Topic initialization script for CLUSTER_NAME

set -e

BOOTSTRAP_SERVERS=${KAFKA_BOOTSTRAP_SERVERS:-"kafka-1:19092,kafka-2:19092,kafka-3:19092"}
CLUSTER_NAME=${CLUSTER_NAME:-"default-cluster"}

echo "🎯 Initializing topics for cluster: $CLUSTER_NAME"
echo "Bootstrap servers: $BOOTSTRAP_SERVERS"

# Define topics with format: "name:partitions:replication:min-isr:retention-ms"
TOPICS=(
    "user-events:9:3:2:604800000"           # 7 days retention
    "order-events:12:3:2:2592000000"        # 30 days retention  
    "notification-events:6:3:2:259200000"   # 3 days retention
    "audit-logs:15:3:2:7776000000"          # 90 days retention
    "helpdesk-tickets:9:3:2:2592000000"     # 30 days retention
    "survey-responses:12:3:2:5184000000"    # 60 days retention
)

create_topic() {
    local topic_config=$1
    IFS=':' read -r topic_name partitions replication min_isr retention <<< "$topic_config"
    
    echo "📋 Creating topic: $topic_name"
    echo "  Partitions: $partitions, Replication: $replication, Min ISR: $min_isr"
    
    kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS \
        --create \
        --topic "$topic_name" \
        --partitions "$partitions" \
        --replication-factor "$replication" \
        --config min.insync.replicas="$min_isr" \
        --config retention.ms="$retention" \
        --if-not-exists
    
    echo "✅ Topic $topic_name created successfully"
}

echo "🚀 Starting topic creation for $CLUSTER_NAME cluster..."

for topic in "${TOPICS[@]}"; do
    create_topic "$topic"
done

echo "🎉 All topics created successfully for cluster: $CLUSTER_NAME!"

# List all topics to verify
echo "📋 Current topics in cluster:"
kafka-topics --bootstrap-server $BOOTSTRAP_SERVERS --list
EOF
        chmod +x "$cluster_dir/init-topics-cluster.sh"
    fi
    
    if [ ! -f "$cluster_dir/monitor-cluster.sh" ]; then
        echo "📝 Creating template monitor-cluster.sh for $cluster_name..."
        cat > "$cluster_dir/monitor-cluster.sh" << 'EOF'
#!/bin/bash
# Cluster monitoring script for CLUSTER_NAME

set -e

BOOTSTRAP_SERVERS=${KAFKA_BOOTSTRAP_SERVERS:-"kafka-1:19092,kafka-2:19092,kafka-3:19092"}
MONITOR_INTERVAL=${MONITOR_INTERVAL:-30}
CLUSTER_NAME=${CLUSTER_NAME:-"default-cluster"}
LOG_DIR="/var/log/kafka-monitor"
LOG_FILE="$LOG_DIR/cluster-monitor.log"

# Create log directory
mkdir -p "$LOG_DIR"

echo "🎯 Kafka Cluster Monitor Starting..."
echo "Cluster: $CLUSTER_NAME"
echo "Bootstrap servers: $BOOTSTRAP_SERVERS"
echo "Monitor interval: ${MONITOR_INTERVAL}s"

log_with_timestamp() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') [$CLUSTER_NAME] - $1" | tee -a "$LOG_FILE"
}

check_cluster_health() {
  log_with_timestamp "🔍 Checking cluster health..."
  
  # Check broker count
  local broker_count=$(kafka-broker-api-versions --bootstrap-server $BOOTSTRAP_SERVERS 2>/dev/null | wc -l || echo 0)
  log_with_timestamp "📊 Active brokers: $broker_count/3"
  
  if [ $broker_count -lt 3 ]; then
    log_with_timestamp "⚠️  WARNING: Only $broker_count/3 brokers available!"
  else
    log_with_timestamp "✅ All 3 brokers are healthy"
  fi
  
  return 0
}

# Trap signals for graceful shutdown
trap 'log_with_timestamp "📴 Kafka monitor shutting down..."; exit 0' SIGTERM SIGINT

log_with_timestamp "🚀 Cluster monitoring started for: $CLUSTER_NAME"

while true; do
  check_cluster_health
  log_with_timestamp "😴 Sleeping for ${MONITOR_INTERVAL} seconds..."
  sleep $MONITOR_INTERVAL
done
EOF
        chmod +x "$cluster_dir/monitor-cluster.sh"
    fi
    
    echo "✅ Cluster structure created for: $cluster_name"
    echo "   📄 Scripts location: $cluster_dir/"
    echo "   📄 init-topics-cluster.sh"
    echo "   📄 monitor-cluster.sh"
}

# Function to show current structure
show_structure() {
    echo ""
    echo "📁 Current Kafka directory structure:"
    echo "====================================="
    
    if [ -d "$KAFKA_DIR" ]; then
        tree "$KAFKA_DIR" 2>/dev/null || find "$KAFKA_DIR" -type f -exec echo "📄 {}" \;
    else
        echo "❌ Kafka directory not found: $KAFKA_DIR"
    fi
}

# Main execution
main() {
    echo "🗂️  Kafka Cluster Directory Setup"
    echo "================================="
    
    # Setup kraft-cluster-1 (move existing files)
    create_cluster_structure "kraft-cluster-1"
    
    # Show final structure
    show_structure
    
    echo ""
    echo "🎯 Setup completed!"
    echo "📝 To manage clusters, use:"
    echo "   ./kafka-cluster-manager.sh setup <cluster-name>"
    echo "   ./kafka-cluster-manager.sh list"
    echo "   ./kafka-cluster-manager.sh help"
    echo ""
    echo "📋 Available clusters:"
    for dir in "$KAFKA_DIR"/*/; do
        if [ -d "$dir" ]; then
            cluster_name=$(basename "$dir")
            echo "   - $cluster_name"
        fi
    done
}

# Handle command line arguments
if [ $# -eq 1 ]; then
    # Create specific cluster
    create_cluster_structure "$1"
    show_structure
else
    # Run full setup
    main
fi
