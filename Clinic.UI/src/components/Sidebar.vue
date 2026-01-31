<script setup>
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { HugeiconsIcon } from '@hugeicons/vue'
import {
  DashboardCircleIcon,
  UserListIcon,
  StethoscopeIcon,
  Calendar01Icon,
  Hospital01Icon,
  CreditCardIcon,
  Settings01Icon,
  Logout01Icon
} from '@hugeicons/core-free-icons'

const router = useRouter()
const route = useRoute()

// Menu supports nested children for submenus. Icons can be any string (emoji or SVG/html).
const menuItems = [
  { icon: DashboardCircleIcon, name: 'Dashboard', path: '/' },
  { icon: UserListIcon, name: 'Patients', path: '/patients' },
  { icon: StethoscopeIcon, name: 'Staff', path: '/staff', children: [
      { name: 'All Staff', path: '/staff' },
      { name: 'Staff Types', path: '/staff-types' }
    ]
  },
  { icon: Calendar01Icon, name: 'Appointments', path: '/appointments' },
  { icon: Hospital01Icon, name: 'Services', path: '/services', children: [
      { name: 'All Services', path: '/services' },
      { name: 'Service Types', path: '/service-types' }
    ]
  },
  { icon: CreditCardIcon, name: 'Payments', path: '/payments' },
]

const configItems = []

const bottomItems = [
  { icon: Settings01Icon, name: 'Settings', path: '/settings' },
  { icon: Logout01Icon, name: 'Logout', action: 'logout' },
]

const authStore = useAuthStore()

// Filter items based on permissions
const visibleMenuItems = computed(() => {
    return menuItems.filter(item => authStore.hasPermission(item.path))
})

const visibleConfigItems = computed(() => {
    return configItems.filter(item => authStore.hasPermission(item.path))
})

const expanded = ref({})

const toggleExpand = (name) => {
  expanded.value[name] = !expanded.value[name]
}

const handleItemClick = (item) => {
  if (item.action === 'logout') {
    authStore.logout()
    router.push('/login')
  } else if (item.children && item.children.length) {
    toggleExpand(item.name)
  } else if (item.path) {
    router.push(item.path)
  }
}

const isActive = (path) => {
  return route.path === path
}
</script>

<template>
  <aside class="sidebar-container expanded">
    <div class="sidebar-logo">Q</div>

      <div>
        <div v-for="item in visibleMenuItems" :key="item.name" class="sidebar-row"
          :class="{ active: isActive(item.path) }">
          <button class="sidebar-button" @click="handleItemClick(item)">
            <div style="display: flex;align-items: center;  gap: 7px;">

              <span class="sidebar-icon">
                <HugeiconsIcon :icon="item.icon" :size="18" color="currentColor" />
              </span>
              <span class="sidebar-text">{{ item.name }}</span>
            </div>
            <span v-if="item.children" class="caret">{{ expanded[item.name] ? '▾' : '▸' }}</span>
          </button>

          <div v-if="item.children && expanded[item.name]" class="submenu">
            <button v-for="child in item.children" :key="child.name" class="submenu-item"
              @click="() => router.push(child.path)">
              {{ child.name }}
            </button>
          </div>
        </div>
      </div>
    <div class="">
      <div
        v-for="item in bottomItems" 
        :key="item.name"
        class="sidebar-row"
      >
        <button class="sidebar-button" style="d" @click="handleItemClick(item)">
          <div>

            <span class="sidebar-icon">
              <HugeiconsIcon :icon="item.icon" :size="18" color="currentColor" />
            </span>
            <span class="sidebar-text">{{ item.name }}</span>
          </div>
        </button>
      </div>
    </div>

  </aside>
</template>

<style scoped>
.sidebar {
  width: 200px;
  min-width: 220px;
  background-color: var(--color-sidebar-bg);
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: var(--spacing-md) 0;
}

.sidebar-container{
  background-color: var(--color-sidebar-bg);
  /* display: flex; */
   width: 220px;
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
  padding:0px;
  padding-right: 60px;

}

.sidebar-row {
  width: 100%;
  flex-direction: column;
  align-items: stretch;
}

.sidebar-button {
  display: flex;
  align-items: center;
  justify-content: space-between;
width: 100%;
  gap: 12px;
  padding: 10px 14px;
  border-radius: 8px;
  color: var(--color-sidebar-text);
  background: transparent;
  border: none;
  cursor: pointer;
  text-align: left;
}
.sidebar-icon { font-size: 28px; width: 36px; display: inline-flex; align-items: center; justify-content: center; }
.sidebar-text { font-weight: 600; }

.submenu { margin-left: 48px; display: flex; flex-direction: column; gap: 6px; margin-top: 6px; }
.submenu-item { background: transparent; border: none; padding: 6px 8px; text-align: left;; cursor: pointer; color: var(--color-sidebar-text-muted); display: block; width: 100%; }

.sidebar-item:hover,
.sidebar-item.active {
  background-color: var(--color-sidebar-active);
  border-left: 1px solid var(--color-primary)
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

.sidebar-bottom { width: 100%; display: flex; flex-direction: column; gap: var(--spacing-sm); padding: 12px; }

.sidebar-button:hover,
.sidebar-button:focus,
.sidebar-row.active .sidebar-button { background-color: var(--color-sidebar-active); }
</style>
