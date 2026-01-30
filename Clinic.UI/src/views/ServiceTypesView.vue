<script setup>
import { ref, onMounted, computed } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { serviceTypesApi, servicesApi } from '../services/api'

const serviceTypes = ref([])
const services = ref([])
const loading = ref(true)
const error = ref(null)
const searchQuery = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const selectedItem = ref(null)

const formData = ref({
  ServiceTypeName: '',
  Cost: 0,
  Description: '',
  ServiceId: null
})

const filteredItems = computed(() => {
  if (!searchQuery.value) return serviceTypes.value
  const query = searchQuery.value.toLowerCase()
  return serviceTypes.value.filter(s => 
    s.ServiceTypeName?.toLowerCase().includes(query) || 
    s.ServiceName?.toLowerCase().includes(query)
  )
})

onMounted(async () => {
  await fetchData()
})

const fetchData = async () => {
  loading.value = true
  error.value = null
  try {
    const [typesRes, servicesRes] = await Promise.all([
      serviceTypesApi.getAll(),
      servicesApi.getAll()
    ])
    serviceTypes.value = typesRes.data
    services.value = servicesRes.data
  } catch (e) {
    error.value = 'Failed to load service types'
    console.error(e)
  } finally {
    loading.value = false
  }
}

const openAddModal = () => {
  isEditing.value = false
  formData.value = { 
    ServiceTypeName: '', 
    Cost: 0, 
    Description: '',
    ServiceId: services.value[0]?.id || null 
  }
  showModal.value = true
}

const openEditModal = (item) => {
  isEditing.value = true
  selectedItem.value = item
  formData.value = { 
    ServiceTypeName: item.ServiceTypeName, 
    Cost: item.Cost, 
    Description: item.Description || '',
    ServiceId: item.ServiceId 
  }
  showModal.value = true
}

const handleSubmit = async () => {
  try {
    const data = { 
      ServiceTypeName: formData.value.ServiceTypeName,
      Cost: parseFloat(formData.value.Cost),
      Description: formData.value.Description || null,
      ServiceId: formData.value.ServiceId
    }
    
    if (isEditing.value) {
      await serviceTypesApi.update(selectedItem.value.id, data)
    } else {
      await serviceTypesApi.create(data)
    }
    showModal.value = false
    await fetchData()
  } catch (e) {
    console.error('Failed to save service type:', e)
  }
}

const handleDelete = async (item) => {
  if (!confirm(`Are you sure you want to delete "${item.ServiceTypeName}"?`)) return
  try {
    await serviceTypesApi.delete(item.id)
    await fetchData()
  } catch (e) {
    console.error('Failed to delete service type:', e)
  }
}

const formatCurrency = (amount) => '$' + (amount || 0).toFixed(2)
</script>

<template>
  <MainLayout>
    <div class="page-header">
      <h1>Service Types</h1>
      <button class="btn btn-primary" @click="openAddModal">+ Add Service Type</button>
    </div>

    <div class="card">
      <div class="table-header">
        <div class="search-input">
          <span>🔍</span>
          <input v-model="searchQuery" type="text" placeholder="Search service types..." />
        </div>
      </div>

      <div v-if="loading" class="loading-state">Loading service types...</div>
      <div v-else-if="error" class="error-state">{{ error }}</div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Service</th>
            <th>Price</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredItems.length === 0">
            <td colspan="5" class="empty-state">No service types found</td>
          </tr>
          <tr v-for="item in filteredItems" :key="item.id">
            <td>{{ item.id }}</td>
            <td>{{ item.ServiceTypeName }}</td>
            <td>{{ item.ServiceName || '-' }}</td>
            <td>{{ formatCurrency(item.Cost) }}</td>
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
          <h2>{{ isEditing ? 'Edit' : 'Add' }} Service Type</h2>
          <button class="btn-close" @click="showModal = false">×</button>
        </div>
        <form @submit.prevent="handleSubmit" class="modal-body">
          <div class="form-group">
            <label>Name *</label>
            <input v-model="formData.ServiceTypeName" type="text" required />
          </div>
          <div class="form-group">
            <label>Service *</label>
            <select v-model="formData.ServiceId" required>
              <option v-for="svc in services" :key="svc.id" :value="svc.id">{{ svc.serviceName }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Price *</label>
            <input v-model="formData.Cost" type="number" step="0.01" required />
          </div>
          <div class="form-group">
            <label>Description</label>
            <textarea v-model="formData.Description" rows="4"></textarea>
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
.form-group input, .form-group select, .form-group textarea { padding: var(--spacing-sm); border: 1px solid #E5E7EB; border-radius: var(--radius-sm); font-size: 0.875rem; }
</style>
