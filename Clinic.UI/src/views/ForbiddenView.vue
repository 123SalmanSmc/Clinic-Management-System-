<template>
  <div class="forbidden">
    <h1>403 Forbidden</h1>
    <p>You do not have permission to access this page.</p>
    <router-link to="/">Go to Dashboard</router-link>
  </div>
</template>

<style scoped>
.forbidden {
  display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100vh;
}
</style>
