<script setup>
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const route = useRoute()

const menuItems = [
  { icon: '📊', name: 'Dashboard', path: '/' },
  { icon: '👥', name: 'Patients', path: '/patients' },
  { icon: '👨‍⚕️', name: 'Staff', path: '/staff' },
  { icon: '📋', name: 'Appointments', path: '/appointments' },
  { icon: '🏥', name: 'Services', path: '/services' },
  { icon: '💳', name: 'Payments', path: '/payments' },
]

const configItems = [
  { icon: '🏷️', name: 'Staff Types', path: '/staff-types' },
  { icon: '🔬', name: 'Specializations', path: '/specializations' },
  { icon: '📦', name: 'Service Types', path: '/service-types' },
  { icon: '💰', name: 'Tax Settings', path: '/tax' },
  { icon: '👤', name: 'Users', path: '/users' },
  { icon: '🛡️', name: 'Roles', path: '/roles' },
]

const bottomItems = [
  { icon: '⚙️', name: 'Settings', path: '/settings' },
  { icon: '🚪', name: 'Logout', action: 'logout' },
]

const authStore = useAuthStore()

// Filter items based on permissions
const visibleMenuItems = computed(() => {
    return menuItems.filter(item => authStore.hasPermission(item.path))
})

const visibleConfigItems = computed(() => {
    return configItems.filter(item => authStore.hasPermission(item.path))
})

const handleItemClick = (item) => {
  if (item.action === 'logout') {
    authStore.logout()
    router.push('/login')
  } else if (item.path) {
    router.push(item.path)
  }
}

const isActive = (path) => {
  return route.path === path
}
</script>

<template>
  <aside class="sidebar">
    <div class="sidebar-logo">Q</div>
    
    <nav class="sidebar-nav">
      <div 
        v-for="item in visibleMenuItems" 
        :key="item.name"
        class="sidebar-item"
        :class="{ active: isActive(item.path) }"
        @click="handleItemClick(item)"
        :title="item.name"
      >
        {{ item.icon }}
      </div>
      
      <div v-if="visibleConfigItems.length > 0" class="sidebar-divider"></div>
      
      <div 
        v-for="item in visibleConfigItems" 
        :key="item.name"
        class="sidebar-item config"
        :class="{ active: isActive(item.path) }"
        @click="handleItemClick(item)"
        :title="item.name"
      >
        {{ item.icon }}
      </div>
    </nav>
    
    <div class="sidebar-bottom">
      <div 
        v-for="item in bottomItems" 
        :key="item.name"
        class="sidebar-item"
        :class="{ active: isActive(item.path) }"
        @click="handleItemClick(item)"
        :title="item.name"
      >
        {{ item.icon }}
      </div>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 72px;
  min-width: 72px;
  background-color: var(--color-sidebar-bg);
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: var(--spacing-md) 0;
}

.sidebar-logo {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, var(--color-primary), #EC4899);
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: bold;
  font-size: 1.25rem;
  margin-bottom: var(--spacing-lg);
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  flex: 1;
}

.sidebar-item {
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-sm);
  color: var(--color-sidebar-text);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 1.25rem;
}

.sidebar-item:hover,
.sidebar-item.active {
  background-color: var(--color-sidebar-active);
}

.sidebar-bottom {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.sidebar-divider {
  width: 32px;
  height: 1px;
  background-color: rgba(255, 255, 255, 0.2);
  margin: var(--spacing-sm) 0;
}

.sidebar-item.config {
  font-size: 1rem;
  opacity: 0.8;
}

.sidebar-item.config:hover {
  opacity: 1;
}
</style>
