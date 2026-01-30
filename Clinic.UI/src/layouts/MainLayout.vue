<script setup>
import Sidebar from '../components/Sidebar.vue'
import TopNav from '../components/TopNav.vue'
</script>

<template>
  <div class="main-layout">
    <Sidebar />
    <div class="content-wrapper">
      <TopNav />
      <main class="main-content">
        <slot></slot>
      </main>
    </div>
  </div>
</template>

<style scoped>
.main-layout {
  display: flex;
  height: 100vh;
  overflow: hidden;
}

.content-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background-color: var(--color-bg-main);
}

.main-content {
  flex: 1;
  overflow-y: auto;
  padding: var(--spacing-lg);
}
</style>
