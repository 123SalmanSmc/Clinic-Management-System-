<script setup>
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

const searchQuery = ref('')
const showProfileMenu = ref(false)

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <header class="topnav">
    <div class="search-input">
      <span>🔍</span>
      <input 
        v-model="searchQuery" 
        type="text" 
        placeholder="Search ..."
      />
    </div>
    
    <div class="topnav-right">
      <button class="icon-btn" title="Help">
        <span>❓</span>
        <span class="badge"></span>
      </button>
      <button class="icon-btn" title="Notifications">
        <span>🔔</span>
        <span class="badge"></span>
      </button>
      
      <div class="profile-wrapper">
        <button 
          class="profile-btn" 
          @click="showProfileMenu = !showProfileMenu"
        >
          <img 
            src="https://ui-avatars.com/api/?name=Admin&background=7C3AED&color=fff" 
            alt="Profile" 
            class="profile-avatar"
          />
          <span class="dropdown-arrow">▼</span>
        </button>
        
        <div v-if="showProfileMenu" class="profile-menu">
          <a href="#" class="menu-item">Profile</a>
          <a href="#" class="menu-item">Settings</a>
          <hr />
          <button @click="handleLogout" class="menu-item logout">Logout</button>
        </div>
      </div>
    </div>
  </header>
</template>

<style scoped>
.topnav {
  height: 64px;
  background-color: var(--color-bg-card);
  border-bottom: 1px solid #E5E7EB;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--spacing-lg);
}

.search-input {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  background-color: var(--color-bg-main);
  border-radius: var(--radius-md);
  padding: var(--spacing-sm) var(--spacing-md);
  min-width: 280px;
}

.search-input input {
  border: none;
  background: transparent;
  outline: none;
  flex: 1;
  font-size: 0.875rem;
  color: var(--color-text-primary);
}

.search-input input::placeholder {
  color: var(--color-text-muted);
}

.topnav-right {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.icon-btn {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  border: none;
  background: transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  font-size: 1.1rem;
}

.icon-btn:hover {
  background-color: var(--color-bg-main);
}

.badge {
  position: absolute;
  top: 8px;
  right: 8px;
  width: 8px;
  height: 8px;
  background-color: var(--color-error);
  border-radius: 50%;
}

.profile-wrapper {
  position: relative;
}

.profile-btn {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  background: transparent;
  border: none;
  cursor: pointer;
  padding: var(--spacing-xs);
  border-radius: var(--radius-md);
}

.profile-btn:hover {
  background-color: var(--color-bg-main);
}

.profile-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
}

.dropdown-arrow {
  font-size: 0.5rem;
  color: var(--color-text-muted);
}

.profile-menu {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: var(--spacing-sm);
  background-color: var(--color-bg-card);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  min-width: 160px;
  padding: var(--spacing-sm);
  z-index: 100;
}

.menu-item {
  display: block;
  padding: var(--spacing-sm) var(--spacing-md);
  color: var(--color-text-primary);
  border-radius: var(--radius-sm);
  width: 100%;
  text-align: left;
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 0.875rem;
}

.menu-item:hover {
  background-color: var(--color-bg-main);
}

.menu-item.logout {
  color: var(--color-error);
}

hr {
  border: none;
  border-top: 1px solid #E5E7EB;
  margin: var(--spacing-sm) 0;
}
</style>
