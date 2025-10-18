#!/bin/bash

# 🚀 3-Broker Kafka Cluster Deployment Script
# Script này sẽ triển khai cụm Kafka 3-broker với monitoring và health checks

set -e

echo "🎯 Starting 3-Broker Kafka Cluster Deployment..."
echo "================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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

# Function to check if Docker is running
check_docker() {
    log_info "Checking Docker status..."
    if ! docker info > /dev/null 2>&1; then
        log_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    log_success "Docker is running"
}

# Function to check if docker-compose is available
check_docker_compose() {
    log_info "Checking Docker Compose..."
    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        log_error "Docker Compose is not available. Please install Docker Compose."
        exit 1
    fi
    log_success "Docker Compose is available"
}

# Function to setup cluster directories
setup_cluster_directories() {
    log_info "Setting up cluster directory structure..."
    
    local setup_script="./setup-cluster-dirs.sh"
    if [ -f "$setup_script" ]; then
        chmod +x "$setup_script"
        if "$setup_script"; then
            log_success "Cluster directories initialized"
        else
            log_warning "Could not run cluster directory setup"
        fi
    else
        log_warning "Cluster directory setup script not found: $setup_script"
    fi
}

# Function to cleanup previous containers
cleanup_previous() {
    log_info "Cleaning up previous Kafka containers..."
    
    # Stop and remove existing Kafka containers
    containers=("kafka-1" "kafka-2" "kafka-3" "kafka-monitor" "kafka-ui-dev-001")
    
    for container in "${containers[@]}"; do
        if docker ps -a --format "table {{.Names}}" | grep -q "^${container}$"; then
            log_warning "Stopping and removing container: $container"
            docker stop "$container" 2>/dev/null || true
            docker rm "$container" 2>/dev/null || true
        fi
    done
    
    log_success "Cleanup completed"
}

# Function to deploy the 3-broker cluster
deploy_cluster() {
    log_info "Deploying 3-broker Kafka cluster..."
    
    # Go to project root for docker-compose commands
    cd ../..
    
    # Start infrastructure services first
    log_info "Starting infrastructure services (Redis, Consul)..."
    docker-compose up -d redis consul
    
    # Wait a bit for infrastructure to be ready
    sleep 10
    
    # Start all 3 Kafka brokers
    log_info "Starting 3 Kafka brokers..."
    docker-compose up -d kafka-1 kafka-2 kafka-3
    
    # Wait for brokers to be ready
    log_info "Waiting for brokers to be ready (30 seconds)..."
    sleep 30
    
    # Initialize topics
    log_info "Initializing topics for 3-broker cluster..."
    docker-compose up kafka-init
    
    # Start monitoring service
    log_info "Starting cluster monitoring service..."
    docker-compose up -d kafka-monitor
    
    # Start Kafka UI
    log_info "Starting Kafka UI..."
    docker-compose up -d kafka-ui
    
    # Return to kafka directory
    cd docker/kafka
    
    log_success "3-broker Kafka cluster deployment completed!"
}

# Function to check cluster health
check_cluster_health() {
    log_info "Checking cluster health..."
    
    # Wait a bit for services to stabilize
    sleep 15
    
    # Check if all brokers are running
    local running_brokers=0
    for i in {1..3}; do
        if docker ps --format "table {{.Names}}" | grep -q "kafka-$i"; then
            if [ "$(docker inspect -f '{{.State.Running}}' kafka-$i)" = "true" ]; then
                running_brokers=$((running_brokers + 1))
                log_success "Broker kafka-$i is running"
            else
                log_warning "Broker kafka-$i is not running properly"
            fi
        else
            log_warning "Broker kafka-$i container not found"
        fi
    done
    
    log_info "Running brokers: $running_brokers/3"
    
    # Check if monitoring is running
    if docker ps --format "table {{.Names}}" | grep -q "kafka-monitor"; then
        log_success "Kafka monitoring service is running"
    else
        log_warning "Kafka monitoring service is not running"
    fi
    
    # Check if Kafka UI is running
    if docker ps --format "table {{.Names}}" | grep -q "kafka-ui-dev-001"; then
        log_success "Kafka UI is running at http://localhost:8080"
    else
        log_warning "Kafka UI is not running"
    fi
}

# Function to show cluster information
show_cluster_info() {
    echo ""
    echo "🔍 Cluster Information:"
    echo "======================"
    echo "• Kafka Brokers:"
    echo "  - kafka-1: localhost:9092"
    echo "  - kafka-2: localhost:9093"
    echo "  - kafka-3: localhost:9094"
    echo ""
    echo "• Management Tools:"
    echo "  - Kafka UI: http://localhost:8080"
    echo "  - Consul: http://localhost:8500"
    echo ""
    echo "• Internal Bootstrap Servers:"
    echo "  - kafka-1:19092,kafka-2:19092,kafka-3:19092"
    echo ""
    echo "• Monitoring:"
    echo "  - Container: kafka-monitor"
    echo "  - Logs: docker logs kafka-monitor -f"
    echo ""
    echo "• Useful Commands:"
    echo "  - View cluster status: docker-compose ps"
    echo "  - View monitor logs: docker logs kafka-monitor -f"
    echo "  - Stop cluster: docker-compose down"
    echo "  - View topics: docker exec kafka-1 kafka-topics --bootstrap-server kafka-1:19092 --list"
    echo ""
    echo "📁 Kafka Scripts Location:"
    echo "  - All scripts: docker/kafka/"
    echo "  - Cluster configs: docker/kafka/kraft-cluster-1/"
    echo "  - Setup script: docker/kafka/setup-cluster-dirs.sh"
    echo "  - Management: docker/kafka/kafka-cluster-manager.sh"
}

# Function to show next steps
show_next_steps() {
    echo ""
    echo "🎯 Next Steps:"
    echo "============="
    echo "1. Access Kafka UI at http://localhost:8080 to view cluster status"
    echo "2. Monitor cluster health: docker logs kafka-monitor -f"
    echo "3. Start your microservices: docker-compose up -d"
    echo "4. Test topic creation and message production"
    echo ""
    echo "📊 To view real-time monitoring:"
    echo "   docker logs kafka-monitor -f"
    echo ""
    echo "🔧 To troubleshoot issues:"
    echo "   docker-compose logs kafka-1"
    echo "   docker-compose logs kafka-2"
    echo "   docker-compose logs kafka-3"
    echo ""
    echo "📁 To manage clusters:"
    echo "   ./kafka-cluster-manager.sh <command>"
    echo "   ./setup-cluster-dirs.sh <new-cluster-name>"
    echo ""
    echo "🎯 Script locations:"
    echo "   - Main deployment: docker/kafka/deploy-kafka-cluster.sh"
    echo "   - Cluster management: docker/kafka/kafka-cluster-manager.sh"
}

# Main execution
main() {
    echo "🚀 3-Broker Kafka Cluster Deployment"
    echo "====================================="
    echo "📁 Running from: docker/kafka/"
    echo "🏠 Project root: ../../"
    
    check_docker
    check_docker_compose
    
    # Ask for confirmation
    echo ""
    read -p "Do you want to proceed with 3-broker cluster deployment? (y/N): " -n 1 -r
    echo ""
    
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        setup_cluster_directories
        cleanup_previous
        deploy_cluster
        check_cluster_health
        show_cluster_info
        show_next_steps
        
        log_success "3-broker Kafka cluster is ready! 🎉"
    else
        log_info "Deployment cancelled by user."
        exit 0
    fi
}

# Run main function
main "$@"
