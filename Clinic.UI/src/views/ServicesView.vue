<script setup>
import { ref, onMounted } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { servicesApi, serviceTypesApi } from '../services/api'

const services = ref([])
const loading = ref(true)
const error = ref(null)
const showModal = ref(false)
const isEditing = ref(false)
const selectedService = ref(null)

const formData = ref({
  serviceName: '',
  description: ''
})

onMounted(async () => {
  await fetchServices()
})

const fetchServices = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await servicesApi.getAll()
    services.value = response.data
  } catch (e) {
    error.value = 'Failed to load services'
    console.error(e)
  } finally {
    loading.value = false
  }
}

const openAddModal = () => {
  isEditing.value = false
  formData.value = { serviceName: '', description: '' }
  showModal.value = true
}

const openEditModal = (service) => {
  isEditing.value = true
  selectedService.value = service
  formData.value = { serviceName: service.serviceName, description: service.description || '' }
  showModal.value = true
}

const handleSubmit = async () => {
  try {
    if (isEditing.value) {
      await servicesApi.update(selectedService.value.id, formData.value)
    } else {
      await servicesApi.create(formData.value)
    }
    showModal.value = false
    formData.value = { serviceName: '', description: '' }
    await fetchServices()
  } catch (e) {
    console.error('Failed to save service:', e)
  }
}

const handleDelete = async (service) => {
  if (!confirm(`Are you sure you want to delete "${service.serviceName}"?`)) return
  try {
    await servicesApi.delete(service.id)
    await fetchServices()
  } catch (e) {
    console.error('Failed to delete service:', e)
  }
}
</script>

<template>
  <MainLayout>
    <div class="page-header">
      <h1>Services</h1>
      <button class="btn btn-primary" @click="openAddModal">
        + Add Service
      </button>
    </div>

    <div v-if="loading" class="loading-state">
      Loading services...
    </div>

    <div v-else-if="error" class="error-state">
      {{ error }}
      <button @click="fetchServices" class="btn btn-secondary">Retry</button>
    </div>

    <div v-else class="table-responsive">
  <table class="data-table">
    <thead>
      <tr>
        <th>ID</th>
        <th>Service Name</th>
        <th>Description</th>
        <th>Actions</th>
      </tr>
    </thead>

    <tbody>
      <tr v-for="service in services" :key="service.id">
        <td>{{ service.id }}</td>
        <td>{{ service.serviceName }}</td>
        <td>
          {{ service.description || 'No description' }}
        </td>
        <td class="actions">
          <button
            class="btn-icon"
            title="Edit"
            @click="openEditModal(service)"
          >
            ✏️
          </button>
          <button
            class="btn-icon"
            title="Delete"
            @click="handleDelete(service)"
          >
            🗑️
          </button>
        </td>
      </tr>

      <!-- Empty State -->
      <tr v-if="services.length === 0">
        <td colspan="4" class="empty-state">
          No services found. Add your first service to get started.
        </td>
      </tr>
    </tbody>
  </table>
</div>


    <!-- Add Service Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Edit' : 'Add' }} Service</h2>
          <button class="btn-close" @click="showModal = false">×</button>
        </div>
        <form @submit.prevent="handleSubmit" class="modal-body">
          <div class="form-group">
            <label>Service Name *</label>
            <input v-model="formData.serviceName" type="text" required />
          </div>
          <div class="form-group">
            <label>Description</label>
            <textarea v-model="formData.description" rows="4"></textarea>
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
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-lg);
}

.services-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--spacing-md);
}

.service-card {
  display: flex;
  flex-direction: column;
}

.service-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-sm);
}

.service-header h3 {
  font-size: 1.125rem;
}

.service-actions {
  display: flex;
  gap: var(--spacing-xs);
}

.service-description {
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  flex: 1;
}

.service-footer {
  margin-top: var(--spacing-md);
  padding-top: var(--spacing-sm);
  border-top: 1px solid #E5E7EB;
}

.service-id {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.btn-icon {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: var(--spacing-xs);
  font-size: 1rem;
}

.loading-state,
.error-state,
.empty-state {
  text-align: center;
  padding: var(--spacing-xl);
  color: var(--color-text-muted);
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background-color: var(--color-bg-card);
  border-radius: var(--radius-lg);
  width: 100%;
  max-width: 500px;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-lg);
  border-bottom: 1px solid #E5E7EB;
}

.btn-close {
  background: transparent;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: var(--color-text-muted);
}

.modal-body {
  padding: var(--spacing-lg);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-sm);
  margin-top: var(--spacing-lg);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  margin-bottom: var(--spacing-md);
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 500;
}

.form-group input,
.form-group textarea {
  padding: var(--spacing-sm);
  border: 1px solid #E5E7EB;
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
}

.form-group input:focus,
.form-group textarea:focus {
  outline: none;
  border-color: var(--color-primary);
}



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

</style>
