#!/bin/bash

# 🎛️ Kafka Cluster Management Script
# Script tổng hợp để quản lý các Kafka clusters

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
KAFKA_DIR="$SCRIPT_DIR"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

log_header() {
    echo -e "${CYAN}$1${NC}"
}

# Function to show usage
show_usage() {
    log_header "🎛️ Kafka Cluster Management"
    echo "================================="
    echo ""
    echo "Usage: $0 [COMMAND] [OPTIONS]"
    echo ""
    echo "Commands:"
    echo "  setup [cluster-name]     Setup cluster directory structure"
    echo "  list                     List all available clusters"
    echo "  info [cluster-name]      Show cluster information"
    echo "  logs [cluster-name]      Show cluster logs"
    echo "  status                   Show running containers status"
    echo "  topics [cluster-name]    List topics in cluster"
    echo "  help                     Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 setup kraft-cluster-1"
    echo "  $0 list"
    echo "  $0 info kraft-cluster-1"
    echo "  $0 logs kraft-cluster-1"
    echo "  $0 status"
    echo "  $0 topics kraft-cluster-1"
    echo ""
    echo "📁 Current location: $KAFKA_DIR"
    echo "🏠 Project root: $PROJECT_ROOT"
}

# Function to setup cluster directories
setup_cluster() {
    local cluster_name=${1:-"kraft-cluster-1"}
    local cluster_dir="$KAFKA_DIR/$cluster_name"
    
    log_info "Setting up cluster: $cluster_name"
    
    # Create cluster directory
    if [ ! -d "$cluster_dir" ]; then
        mkdir -p "$cluster_dir"
        log_success "Created directory: $cluster_dir"
    else
        log_warning "Directory already exists: $cluster_dir"
    fi
    
    # Create init-topics script if not exists
    if [ ! -f "$cluster_dir/init-topics-cluster.sh" ]; then
        log_info "Creating init-topics-cluster.sh for $cluster_name..."
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
        log_success "Created init-topics-cluster.sh"
    fi
    
    # Create monitor script if not exists
    if [ ! -f "$cluster_dir/monitor-cluster.sh" ]; then
        log_info "Creating monitor-cluster.sh for $cluster_name..."
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
        log_success "Created monitor-cluster.sh"
    fi
    
    log_success "Cluster $cluster_name setup completed"
    echo "📁 Location: $cluster_dir"
}

# Function to list all clusters
list_clusters() {
    log_header "📋 Available Kafka Clusters"
    echo "=========================="
    
    local found_clusters=false
    
    for dir in "$KAFKA_DIR"/*/; do
        if [ -d "$dir" ]; then
            local cluster_name=$(basename "$dir")
            if [ "$cluster_name" != "." ] && [ "$cluster_name" != ".." ]; then
                found_clusters=true
                echo "🎯 $cluster_name"
                echo "   📄 init-topics-cluster.sh: $([ -f "$dir/init-topics-cluster.sh" ] && echo "✅" || echo "❌")"
                echo "   📄 monitor-cluster.sh: $([ -f "$dir/monitor-cluster.sh" ] && echo "✅" || echo "❌")"
                echo ""
            fi
        fi
    done
    
    if [ "$found_clusters" = false ]; then
        log_warning "No clusters found in $KAFKA_DIR"
        echo "Run: $0 setup [cluster-name] to create a cluster"
    fi
}

# Function to show cluster info
show_cluster_info() {
    local cluster_name=${1:-"kraft-cluster-1"}
    local cluster_dir="$KAFKA_DIR/$cluster_name"
    
    if [ ! -d "$cluster_dir" ]; then
        log_error "Cluster not found: $cluster_name"
        return 1
    fi
    
    log_header "🔍 Cluster Information: $cluster_name"
    echo "================================="
    echo "📁 Location: $cluster_dir"
    echo "📄 Scripts:"
    echo "   - init-topics-cluster.sh: $([ -f "$cluster_dir/init-topics-cluster.sh" ] && echo "✅ Available" || echo "❌ Missing")"
    echo "   - monitor-cluster.sh: $([ -f "$cluster_dir/monitor-cluster.sh" ] && echo "✅ Available" || echo "❌ Missing")"
    echo ""
    echo "🎛️ Docker Compose Environment:"
    echo "   CLUSTER_NAME: \"$cluster_name\""
    echo ""
    echo "📋 To use this cluster, update docker-compose.yml:"
    echo "   environment:"
    echo "     CLUSTER_NAME: \"$cluster_name\""
}

# Function to show cluster logs
show_cluster_logs() {
    local cluster_name=${1:-"kraft-cluster-1"}
    
    log_info "Showing logs for cluster: $cluster_name"
    
    if docker ps --format "table {{.Names}}" | grep -q "kafka-monitor"; then
        echo "📊 Kafka Monitor Logs:"
        echo "====================="
        docker logs kafka-monitor --tail 50
    else
        log_warning "Kafka monitor container not running"
    fi
    
    echo ""
    echo "🎯 Broker Logs:"
    echo "=============="
    for i in {1..3}; do
        if docker ps --format "table {{.Names}}" | grep -q "kafka-$i"; then
            echo ""
            echo "📋 Kafka-$i logs (last 10 lines):"
            docker logs "kafka-$i" --tail 10
        fi
    done
}

# Function to show container status
show_status() {
    log_header "📊 Kafka Container Status"
    echo "========================="
    
    # Check brokers
    echo "🎯 Kafka Brokers:"
    for i in {1..3}; do
        if docker ps --format "table {{.Names}}" | grep -q "kafka-$i"; then
            local status=$(docker inspect -f '{{.State.Running}}' "kafka-$i" 2>/dev/null || echo "false")
            if [ "$status" = "true" ]; then
                echo "   ✅ kafka-$i: Running"
            else
                echo "   ⚠️  kafka-$i: Not running properly"
            fi
        else
            echo "   ❌ kafka-$i: Container not found"
        fi
    done
    
    echo ""
    echo "🔧 Support Services:"
    
    # Check monitor
    if docker ps --format "table {{.Names}}" | grep -q "kafka-monitor"; then
        echo "   ✅ kafka-monitor: Running"
    else
        echo "   ❌ kafka-monitor: Not running"
    fi
    
    # Check UI
    if docker ps --format "table {{.Names}}" | grep -q "kafka-ui-dev-001"; then
        echo "   ✅ kafka-ui: Running (http://localhost:8080)"
    else
        echo "   ❌ kafka-ui: Not running"
    fi
    
    # Check infrastructure
    if docker ps --format "table {{.Names}}" | grep -q "redis"; then
        echo "   ✅ redis: Running"
    else
        echo "   ❌ redis: Not running"
    fi
    
    if docker ps --format "table {{.Names}}" | grep -q "consul"; then
        echo "   ✅ consul: Running"
    else
        echo "   ❌ consul: Not running"
    fi
}

# Function to list topics in cluster
show_topics() {
    local cluster_name=${1:-"kraft-cluster-1"}
    
    log_info "Listing topics for cluster: $cluster_name"
    
    if docker ps --format "table {{.Names}}" | grep -q "kafka-1"; then
        echo "📋 Topics in cluster:"
        docker exec kafka-1 kafka-topics --bootstrap-server kafka-1:19092,kafka-2:19092,kafka-3:19092 --list
        
        echo ""
        echo "📊 Topic details:"
        docker exec kafka-1 kafka-topics --bootstrap-server kafka-1:19092,kafka-2:19092,kafka-3:19092 --describe
    else
        log_error "Kafka brokers not running. Start cluster first."
    fi
}

# Main execution
main() {
    local command=${1:-"help"}
    local arg2=$2
    
    case $command in
        "setup")
            setup_cluster "$arg2"
            ;;
        "list")
            list_clusters
            ;;
        "info")
            show_cluster_info "$arg2"
            ;;
        "logs")
            show_cluster_logs "$arg2"
            ;;
        "status")
            show_status
            ;;
        "topics")
            show_topics "$arg2"
            ;;
        "help"|*)
            show_usage
            ;;
    esac
}

# Run main function
main "$@"
