<script setup>
import { ref, onMounted } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { rolesApi } from '../services/api'
import { useRouter } from 'vue-router'

const roles = ref([])
const loading = ref(true)
const showModal = ref(false)
const isEditing = ref(false)
const formData = ref({ id: 0, name: '', description: '' })
const router = useRouter()

onMounted(() => fetchRoles())

const fetchRoles = async () => {
  try {
    const res = await rolesApi.getAll()
    roles.value = res.data
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  isEditing.value = false
  formData.value = { id: 0, name: '', description: '' }
  showModal.value = true
}

const openEditModal = (role) => {
  isEditing.value = true
  formData.value = { ...role }
  showModal.value = true
}

const saveRole = async () => {
  try {
    if (isEditing.value) {
      await rolesApi.update(formData.value.id, formData.value)
    } else {
      await rolesApi.create(formData.value)
    }
    showModal.value = false
    fetchRoles()
  } catch (e) {
    alert('Failed to save role')
  }
}

const deleteRole = async (id) => {
  if (!confirm('Delete this role?')) return
  try {
    await rolesApi.delete(id)
    fetchRoles()
  } catch (e) {
    alert(e.response?.data || 'Failed to delete role')
  }
}

const managePermissions = (role) => {
  router.push(`/roles/${role.id}/permissions`)
}
</script>

<template>
  <MainLayout>
    <div class="header">
        <h1>Roles Management</h1>
        <button class="btn btn-primary" @click="openCreateModal">+ New Role</button>
    </div>

    <div class="card">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Description</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="role in roles" :key="role.id">
            <td>{{ role.id }}</td>
            <td>{{ role.name }}</td>
            <td>{{ role.description }}</td>
            <td>
              <button class="btn-icon" title="Permissions" @click="managePermissions(role)">🛡️</button>
              <button class="btn-icon" title="Edit" @click="openEditModal(role)">✏️</button>
              <button class="btn-icon" title="Delete" @click="deleteRole(role.id)" v-if="role.id > 1">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>

       <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal">
        <div class="modal-header" style="display: flex; justify-content: space-between; align-items: center;">
          <h2>{{ isEditing ? 'Edit' : 'Create' }} Role</h2>
          <button class="btn-clos e" @click="showModal = false">×</button>
        </div>
        <div class="modal-body">
            <div class="form-group">
                <label>Name</label>
                <input v-model="formData.name" type="text" required />
            </div>
            <div class="form-group">
                <label>Description</label>
                <textarea v-model="formData.description" rows="3"></textarea>
            </div>
        </div>
        <div class="modal-footer">
            <button class="btn" @click="showModal = false">Cancel</button>
            <button class="btn btn-primary" @click="saveRole">Save</button>
        </div>
      </div>
    </div>
    </div>

    <!-- Modal -->
   
  </MainLayout>
</template>

<style scoped>
.header { display: flex; justify-content: space-between; margin-bottom: 20px; align-items: center; }
.form-group { margin-bottom: 15px; display: flex; flex-direction: column; }
.form-group label { margin-bottom: 5px; font-weight: 500; }
.form-group input, .form-group textarea { padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
.modal-footer { margin-top: 20px; display: flex; justify-content: flex-end; gap: 10px; }



.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th,
.data-table td {
  padding: var(--spacing-sm) var(--spacing-md);
  text-align: left;
  border-bottom: 1px solid #E5E7EB;
}

.data-table th {
  font-weight: 600;
  color: var(--color-text-secondary);
  font-size: 0.75rem;
  text-transform: uppercase;
}

.data-table tr:hover {
  background-color: var(--color-bg-main);
}

.modal{
  margin-top :5%
}

</style>
