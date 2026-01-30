<script setup>
import MainLayout from '../layouts/MainLayout.vue'
</script>

<template>
  <MainLayout>
    <div class="page-header">
      <h1>Settings</h1>
    </div>

    <div class="settings-grid">
      <div class="card">
        <h3>Profile Settings</h3>
        <p class="text-muted">Manage your account settings and preferences.</p>
        <div class="settings-content">
          <div class="form-group">
            <label>Username</label>
            <input type="text" value="admin" disabled />
          </div>
          <div class="form-group">
            <label>Email</label>
            <input type="email" placeholder="admin@clinic.com" />
          </div>
          <button class="btn btn-primary">Update Profile</button>
        </div>
      </div>

      <div class="card">
        <h3>Security</h3>
        <p class="text-muted">Change your password and security settings.</p>
        <div class="settings-content">
          <div class="form-group">
            <label>Current Password</label>
            <input type="password" placeholder="Enter current password" />
          </div>
          <div class="form-group">
            <label>New Password</label>
            <input type="password" placeholder="Enter new password" />
          </div>
          <div class="form-group">
            <label>Confirm Password</label>
            <input type="password" placeholder="Confirm new password" />
          </div>
          <button class="btn btn-primary">Change Password</button>
        </div>
      </div>

      <div class="card">
        <h3>Notifications</h3>
        <p class="text-muted">Configure notification preferences.</p>
        <div class="settings-content">
          <label class="toggle-row">
            <input type="checkbox" checked />
            <span>Email notifications</span>
          </label>
          <label class="toggle-row">
            <input type="checkbox" checked />
            <span>Appointment reminders</span>
          </label>
          <label class="toggle-row">
            <input type="checkbox" />
            <span>Marketing emails</span>
          </label>
        </div>
      </div>

      <div class="card">
        <h3>System</h3>
        <p class="text-muted">System configuration and preferences.</p>
        <div class="settings-content">
          <div class="form-group">
            <label>Default Tax Rate (%)</label>
            <input type="number" value="5" step="0.1" />
          </div>
          <div class="form-group">
            <label>Currency</label>
            <select>
              <option value="USD">USD ($)</option>
              <option value="EUR">EUR (€)</option>
              <option value="GBP">GBP (£)</option>
            </select>
          </div>
          <button class="btn btn-primary">Save Settings</button>
        </div>
      </div>
    </div>
  </MainLayout>
</template>

<style scoped>
.page-header {
  margin-bottom: var(--spacing-lg);
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: var(--spacing-lg);
}

.card h3 {
  margin-bottom: var(--spacing-xs);
}

.card p {
  margin-bottom: var(--spacing-lg);
}

.settings-content {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 500;
}

.form-group input,
.form-group select {
  padding: var(--spacing-sm);
  border: 1px solid #E5E7EB;
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: var(--color-primary);
}

.form-group input:disabled {
  background-color: var(--color-bg-main);
  cursor: not-allowed;
}

.toggle-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  cursor: pointer;
}

.toggle-row input[type="checkbox"] {
  width: 18px;
  height: 18px;
  accent-color: var(--color-primary);
}
</style>
