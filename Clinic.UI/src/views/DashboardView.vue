<script setup>
import { ref, onMounted } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { dashboardApi } from '../services/api'

// Stats data
const stats = ref({
  totalSessions: '2.4k+',
  totalVisitors: '1.6k+',
  timeSpent: '8.49',
  avgRequests: '10.4'
})

const sessionsOverview = ref([])
const topSessions = ref([
  { name: 'Chrome', type: 'Top Browser', value: '2k+', icon: '🌐' },
  { name: 'Windows', type: 'Top Platform', value: '1.9k+', icon: '💻' },
  { name: 'Stack Overflow', type: 'Top Sources', value: '1.6k+', icon: '📚' }
])

const statistics = ref([
  { label: 'Online Visitors', sub: 'Max 878', value: '320', progress: 30 },
  { label: 'Online Visitors', sub: 'Max 878', value: '320', progress: 87 },
  { label: 'Average Revenue', sub: '+21%', value: '3.1k+', progress: 21, isChart: true }
])

const loading = ref(true)
const error = ref(null)

onMounted(async () => {
  try {
    // Try to fetch real data
    const [statsRes, summaryRes] = await Promise.all([
      dashboardApi.getStats().catch(() => null),
      dashboardApi.getSummary().catch(() => null)
    ])

    if (summaryRes?.data) {
      stats.value.totalSessions = formatNumber(summaryRes.data.totalSessions)
      stats.value.totalVisitors = formatNumber(summaryRes.data.totalVisitors)
      stats.value.timeSpent = summaryRes.data.timeSpentHr?.toFixed(2) || '8.49'
      stats.value.avgRequests = summaryRes.data.avgRequestsReceived?.toFixed(1) || '10.4'
    }

    if (statsRes?.data?.sessionsOverview) {
      sessionsOverview.value = statsRes.data.sessionsOverview
    }
  } catch (e) {
    console.error('Failed to load dashboard data:', e)
    // Keep using default demo data
  } finally {
    loading.value = false
  }
})

const formatNumber = (num) => {
  if (num >= 1000) {
    return (num / 1000).toFixed(1) + 'k+'
  }
  return num?.toString() || '0'
}

// Generate chart data points for demo
const chartPoints = ref([
  { x: 0, y: 80 }, { x: 1, y: 100 }, { x: 2, y: 90 }, { x: 3, y: 120 },
  { x: 4, y: 110 }, { x: 5, y: 150 }, { x: 6, y: 200 }, { x: 7, y: 180 },
  { x: 8, y: 220 }, { x: 9, y: 190 }
])

const generatePath = () => {
  const width = 600
  const height = 180
  const padding = 20
  const points = chartPoints.value
  const stepX = (width - 2 * padding) / (points.length - 1)
  const maxY = Math.max(...points.map(p => p.y))
  
  let path = `M ${padding} ${height - padding - (points[0].y / maxY) * (height - 2 * padding)}`
  
  for (let i = 1; i < points.length; i++) {
    const x = padding + i * stepX
    const y = height - padding - (points[i].y / maxY) * (height - 2 * padding)
    path += ` L ${x} ${y}`
  }
  
  return path
}
</script>

<template>
  <MainLayout>
    <!-- Stats Cards Row -->
    <div class="stats-grid">
      <div class="stat-card primary">
        <span class="stat-label text-white">Total Sessions</span>
        <span class="stat-value">{{ stats.totalSessions }}</span>
        <div class="mini-chart">
          <svg viewBox="0 0 100 40" class="mini-chart-svg">
            <path d="M 0 30 Q 20 25, 40 28 T 80 20 T 100 25" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="2"/>
          </svg>
        </div>
        <span class="stat-change positive">
          <span>↑</span> 17%
        </span>
      </div>
      
      <div class="stat-card">
        <span class="stat-label">Total Visitors</span>
        <span class="stat-value">{{ stats.totalVisitors }}</span>
        <span class="stat-change negative">
          <span>↓</span> 17%
        </span>
      </div>
      
      <div class="stat-card">
        <span class="stat-label">Time Spent, Hr</span>
        <span class="stat-value">{{ stats.timeSpent }}</span>
        <span class="stat-change positive">
          <span>↑</span> 24%
        </span>
      </div>
      
      <div class="stat-card">
        <span class="stat-label">AVG Requests Received</span>
        <span class="stat-value">{{ stats.avgRequests }}</span>
        <span class="stat-change positive">
          <span>↑</span> 10%
        </span>
      </div>
    </div>

    <!-- Main Dashboard Grid -->
    <div class="dashboard-grid">
      <!-- Left Column -->
      <div class="dashboard-grid-left">
        <!-- Sessions Overview -->
        <div class="card">
          <div class="chart-header">
            <h3>Sessions Overview</h3>
            <div class="chart-actions">
              <button class="btn btn-secondary">
                Last 10 days
                <span>▼</span>
              </button>
              <button class="btn btn-dark">
                <span>⬇</span> Download CSV
              </button>
            </div>
          </div>
          <div class="chart-container">
            <svg viewBox="0 0 600 200" class="line-chart">
              <!-- Grid lines -->
              <line x1="20" y1="40" x2="580" y2="40" stroke="#E5E7EB" stroke-dasharray="4"/>
              <line x1="20" y1="80" x2="580" y2="80" stroke="#E5E7EB" stroke-dasharray="4"/>
              <line x1="20" y1="120" x2="580" y2="120" stroke="#E5E7EB" stroke-dasharray="4"/>
              <line x1="20" y1="160" x2="580" y2="160" stroke="#E5E7EB" stroke-dasharray="4"/>
              
              <!-- Y-axis labels -->
              <text x="10" y="44" fill="#9CA3AF" font-size="10">300</text>
              <text x="10" y="84" fill="#9CA3AF" font-size="10">200</text>
              <text x="10" y="124" fill="#9CA3AF" font-size="10">100</text>
              <text x="10" y="164" fill="#9CA3AF" font-size="10">0</text>
              
              <!-- Chart line -->
              <path :d="generatePath()" fill="none" stroke="#1E1E2F" stroke-width="2"/>
              
              <!-- Second line (purple) -->
              <path d="M 20 140 Q 80 130, 140 135 T 260 125 T 380 140 T 500 120 T 580 130" 
                    fill="none" stroke="#7C3AED" stroke-width="2"/>
              
              <!-- Tooltip -->
              <g transform="translate(260, 55)">
                <rect x="-25" y="-25" width="50" height="25" rx="4" fill="#1E1E2F"/>
                <text x="0" y="-10" fill="white" font-size="10" text-anchor="middle">↑17%</text>
              </g>
            </svg>
          </div>
        </div>

        <!-- Top Sessions -->
        <div class="card">
          <div class="top-sessions-header">
            <div>
              <h3>Top Sessions</h3>
              <p class="text-muted">Your website statistics for <strong>1 week</strong> period.</p>
            </div>
          </div>
          <div class="session-cards">
            <div v-for="session in topSessions" :key="session.name" class="session-card">
              <div class="session-card-icon">{{ session.icon }}</div>
              <div class="session-card-name">{{ session.name }}</div>
              <div class="session-card-label">{{ session.type }}</div>
              <div class="session-card-value">{{ session.value }} <span class="text-muted">/ Sessions</span></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Right Column -->
      <div class="dashboard-grid-right">
        <!-- Views by Browser -->
        <div class="card">
          <div class="card-header-flex">
            <h3>Views by Browser</h3>
            <button class="icon-btn-small">⋯</button>
          </div>
          <div class="radar-chart-container">
            <svg viewBox="0 0 200 200" class="radar-chart">
              <!-- Radar grid -->
              <polygon points="100,20 180,70 160,160 40,160 20,70" fill="none" stroke="#E5E7EB"/>
              <polygon points="100,40 160,75 145,145 55,145 40,75" fill="none" stroke="#E5E7EB"/>
              <polygon points="100,60 140,80 130,130 70,130 60,80" fill="none" stroke="#E5E7EB"/>
              
              <!-- Data polygon -->
              <polygon points="100,35 165,72 135,150 50,140 30,75" 
                       fill="rgba(124, 58, 237, 0.3)" stroke="#7C3AED" stroke-width="2"/>
              
              <!-- Labels -->
              <text x="100" y="12" text-anchor="middle" fill="#6B7280" font-size="10">Explorer</text>
              <text x="190" y="75" text-anchor="start" fill="#6B7280" font-size="10">Safari</text>
              <text x="165" y="175" text-anchor="middle" fill="#6B7280" font-size="10">Chrome</text>
              <text x="35" y="175" text-anchor="middle" fill="#6B7280" font-size="10">Firefox</text>
            </svg>
          </div>
        </div>

        <!-- Statistics -->
        <div class="card">
          <div class="card-header-flex">
            <h3>Statistics</h3>
            <button class="icon-btn-small">⋯</button>
          </div>
          <div class="statistics-list">
            <div v-for="(stat, index) in statistics" :key="index" class="stat-row">
              <div class="stat-left">
                <div class="circular-progress" :style="{ '--progress': stat.progress + '%' }">
                  <div class="circular-progress-inner">{{ stat.progress }}%</div>
                </div>
                <div class="stat-info">
                  <span class="stat-info-label">{{ stat.label }}</span>
                  <span class="stat-info-sub">{{ stat.sub }}</span>
                </div>
              </div>
              <div class="stat-number">{{ stat.value }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </MainLayout>
</template>

<style scoped>
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--spacing-md);
}

.stat-card {
  background-color: var(--color-bg-card);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  position: relative;
  overflow: hidden;
}

.stat-card.primary {
  background-color: var(--color-sidebar-bg);
  color: var(--color-text-white);
}

.stat-card.primary .stat-label {
  color: rgba(255, 255, 255, 0.7);
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  line-height: 1;
}

.stat-label {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.stat-change {
  font-size: 0.75rem;
  display: flex;
  align-items: center;
  gap: 4px;
}

.stat-change.positive { color: var(--color-success); }
.stat-change.negative { color: var(--color-error); }

.mini-chart {
  position: absolute;
  right: 16px;
  top: 50%;
  transform: translateY(-50%);
  width: 80px;
  height: 40px;
}

.mini-chart-svg {
  width: 100%;
  height: 100%;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: var(--spacing-lg);
  margin-top: var(--spacing-lg);
}

.dashboard-grid-left,
.dashboard-grid-right {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.chart-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--spacing-lg);
}

.chart-header h3 {
  font-size: 1.125rem;
  font-weight: 600;
}

.chart-actions {
  display: flex;
  gap: var(--spacing-sm);
}

.chart-container {
  width: 100%;
  height: 220px;
}

.line-chart {
  width: 100%;
  height: 100%;
}

.top-sessions-header {
  margin-bottom: var(--spacing-lg);
}

.top-sessions-header h3 {
  margin-bottom: var(--spacing-xs);
}

.session-cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-md);
}

.session-card {
  background-color: var(--color-bg-dark);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  color: var(--color-text-white);
  text-align: center;
}

.session-card-icon {
  width: 48px;
  height: 48px;
  margin: 0 auto var(--spacing-sm);
  background-color: rgba(255, 255, 255, 0.1);
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
}

.session-card-name {
  font-weight: 600;
  margin-bottom: var(--spacing-xs);
}

.session-card-label {
  font-size: 0.75rem;
  color: var(--color-sidebar-text);
  margin-bottom: var(--spacing-sm);
}

.session-card-value {
  font-size: 1.25rem;
  font-weight: 700;
}

.session-card-value .text-muted {
  font-size: 0.75rem;
  font-weight: 400;
}

.card-header-flex {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--spacing-md);
}

.icon-btn-small {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 1rem;
}

.icon-btn-small:hover {
  background-color: var(--color-bg-main);
}

.radar-chart-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px;
}

.radar-chart {
  width: 100%;
  height: 100%;
  max-width: 200px;
}

.statistics-list {
  display: flex;
  flex-direction: column;
}

.stat-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md) 0;
  border-bottom: 1px solid #E5E7EB;
}

.stat-row:last-child {
  border-bottom: none;
}

.stat-left {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.circular-progress {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: conic-gradient(var(--color-primary) var(--progress, 30%), #E5E7EB 0);
  display: flex;
  align-items: center;
  justify-content: center;
}

.circular-progress-inner {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background-color: var(--color-bg-card);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.625rem;
  color: var(--color-text-muted);
}

.stat-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.stat-info-label {
  font-size: 0.875rem;
  color: var(--color-text-primary);
}

.stat-info-sub {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.stat-number {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-text-primary);
}

@media (max-width: 1200px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
  
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .session-cards {
    grid-template-columns: 1fr;
  }
}
</style>
