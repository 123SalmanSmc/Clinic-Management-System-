<script setup>
import { ref, onMounted, computed } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { usersApi, rolesApi, staffApi } from '../services/api'

const users = ref([])
const roles = ref([])
const staffList = ref([])
const loading = ref(true)
const error = ref(null)
const searchQuery = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const selectedItem = ref(null)

const formData = ref({
  username: '',
  password: '',
  roleId: 2, // Default to Doctor or similar
  staffId: null
})

const filteredItems = computed(() => {
  if (!searchQuery.value) return users.value
  const query = searchQuery.value.toLowerCase()
  return users.value.filter(u => u.username.toLowerCase().includes(query))
})

onMounted(async () => {
  await fetchData()
})

const fetchData = async () => {
  loading.value = true
  error.value = null
  try {
    const [usersRes, rolesRes, staffRes] = await Promise.all([
      usersApi.getAll(),
      rolesApi.getAll(),
      staffApi.getAll()
    ])
    users.value = usersRes.data
    roles.value = rolesRes.data
    staffList.value = staffRes.data
  } catch (e) {
    error.value = 'Failed to load data'
    console.error(e)
  } finally {
    loading.value = false
  }
}

const openAddModal = () => {
  isEditing.value = false
  formData.value = { username: '', password: '', roleId: roles.value.length > 0 ? roles.value[0].id : null, staffId: null }
  showModal.value = true
}

const openEditModal = (item) => {
  isEditing.value = true
  selectedItem.value = item
  formData.value = { 
      username: item.username, 
      password: '', 
      roleId: item.roleId || roles.value.find(r => r.name === item.role)?.id, 
      staffId: item.staffId 
  }
  showModal.value = true
}

const handleSubmit = async () => {
  try {
    if (isEditing.value) {
      await usersApi.update(selectedItem.value.id, formData.value)
    } else {
      await usersApi.create(formData.value)
    }
    showModal.value = false
    await fetchData()
  } catch (e) {
    console.error('Failed to save user:', e)
    alert(e.response?.data || 'Failed to save user')
  }
}

const handleDelete = async (item) => {
  if (!confirm(`Are you sure you want to delete user "${item.username}"?`)) return
  try {
    await usersApi.delete(item.id)
    await fetchData()
  } catch (e) {
    console.error('Failed to delete user:', e)
  }
}
</script>

<template>
  <MainLayout>
    <div class="page-header">
      <h1>Users</h1>
      <button class="btn btn-primary" @click="openAddModal">+ Add User</button>
    </div>

    <div class="card">
      <div class="table-header">
        <div class="search-input">
          <span>🔍</span>
          <input v-model="searchQuery" type="text" placeholder="Search users..." />
        </div>
      </div>

      <div v-if="loading" class="loading-state">Loading users...</div>
      <div v-else-if="error" class="error-state">{{ error }}</div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Username</th>
            <th>Role</th>
            <th>Staff Member</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredItems.length === 0">
            <td colspan="5" class="empty-state">No users found</td>
          </tr>
          <tr v-for="item in filteredItems" :key="item.id">
            <td>{{ item.id }}</td>
            <td>{{ item.username }}</td>
            <td>
              <span :class="['role-badge', item.role?.toLowerCase()]">{{ item.role }}</span>
            </td>
            <td>{{ item.staffName || '-' }}</td>
            <td>
              <button class="btn-icon" title="Edit" @click="openEditModal(item)">✏️</button>
              <button class="btn-icon" title="Delete" @click="handleDelete(item)">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Edit' : 'Add' }} User</h2>
          <button class="btn-close" @click="showModal = false">×</button>
        </div>
        <form @submit.prevent="handleSubmit" class="modal-body">
          <div class="form-group">
            <label>Username *</label>
            <input v-model="formData.username" type="text" required />
          </div>
          <div class="form-group">
            <label>Password {{ isEditing ? '(leave blank to keep)' : '*' }}</label>
            <input v-model="formData.password" type="password" :required="!isEditing" />
          </div>
          <div class="form-group">
            <label>Role *</label>
            <select v-model="formData.roleId" required>
              <option v-for="role in roles" :key="role.id" :value="role.id">{{ role.name }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Link to Staff Member (Optional)</label>
            <select v-model="formData.staffId">
              <option :value="null">-- None --</option>
              <option v-for="staff in staffList" :key="staff.id" :value="staff.id">{{ staff.fullName }}</option>
            </select>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showModal = false">Cancel</button>
            <button type="submit" class="btn btn-primary">{{ isEditing ? 'Update' : 'Save' }}</button>
          </div>
        </form>
      </div>
    </div>
  </MainLayout>
</template>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--spacing-lg); }
.table-header { margin-bottom: var(--spacing-md); }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th, .data-table td { padding: var(--spacing-sm) var(--spacing-md); text-align: left; border-bottom: 1px solid #E5E7EB; }
.data-table th { font-weight: 600; color: var(--color-text-secondary); font-size: 0.75rem; text-transform: uppercase; }
.data-table tr:hover { background-color: var(--color-bg-main); }
.role-badge { padding: 4px 8px; border-radius: var(--radius-sm); font-size: 0.75rem; background-color: #E0E7FF; color: #3730A3; }
.role-badge.admin { background-color: #FEE2E2; color: #991B1B; }
.role-badge.superadmin { background-color: #FEF3C7; color: #92400E; }
.btn-icon { background: transparent; border: none; cursor: pointer; padding: var(--spacing-xs); font-size: 1rem; }
.loading-state, .error-state, .empty-state { text-align: center; padding: var(--spacing-xl); color: var(--color-text-muted); }
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background-color: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.modal { background-color: var(--color-bg-card); border-radius: var(--radius-lg); width: 100%; max-width: 500px; }
.modal-header { display: flex; justify-content: space-between; align-items: center; padding: var(--spacing-lg); border-bottom: 1px solid #E5E7EB; }
.btn-close { background: transparent; border: none; font-size: 1.5rem; cursor: pointer; color: var(--color-text-muted); }
.modal-body { padding: var(--spacing-lg); }
.modal-footer { display: flex; justify-content: flex-end; gap: var(--spacing-sm); margin-top: var(--spacing-lg); }
.form-group { display: flex; flex-direction: column; gap: var(--spacing-xs); margin-bottom: var(--spacing-md); }
.form-group label { font-size: 0.875rem; font-weight: 500; }
.form-group input, .form-group select { padding: var(--spacing-sm); border: 1px solid #E5E7EB; border-radius: var(--radius-sm); font-size: 0.875rem; }
</style>
