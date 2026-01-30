<script setup>
import { ref, onMounted, computed } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { specializationsApi } from '../services/api'

const specializations = ref([])
const loading = ref(true)
const error = ref(null)
const searchQuery = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const selectedItem = ref(null)

const formData = ref({
  name: '',
  description: '',
  consultationCost: 0
})

const filteredItems = computed(() => {
  if (!searchQuery.value) return specializations.value
  const query = searchQuery.value.toLowerCase()
  return specializations.value.filter(s => s.name.toLowerCase().includes(query))
})

onMounted(async () => {
  await fetchData()
})

const fetchData = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await specializationsApi.getAll()
    specializations.value = response.data
  } catch (e) {
    error.value = 'Failed to load specializations'
    console.error(e)
  } finally {
    loading.value = false
  }
}

const openAddModal = () => {
  isEditing.value = false
  formData.value = { name: '', description: '', consultationCost: 0 }
  showModal.value = true
}

const openEditModal = (item) => {
  isEditing.value = true
  selectedItem.value = item
  formData.value = { name: item.name, description: item.description || '', consultationCost: item.consultationCost || 0 }
  showModal.value = true
}

const handleSubmit = async () => {
  try {
    if (isEditing.value) {
      await specializationsApi.update(selectedItem.value.id, formData.value)
    } else {
      await specializationsApi.create(formData.value)
    }
    showModal.value = false
    await fetchData()
  } catch (e) {
    console.error('Failed to save specialization:', e)
  }
}

const handleDelete = async (item) => {
  if (!confirm(`Are you sure you want to delete "${item.name}"?`)) return
  try {
    await specializationsApi.delete(item.id)
    await fetchData()
  } catch (e) {
    console.error('Failed to delete specialization:', e)
  }
}
</script>

<template>
  <MainLayout>
    <div class="page-header">
      <h1>Specializations</h1>
      <button class="btn btn-primary" @click="openAddModal">+ Add Specialization</button>
    </div>

    <div class="card">
      <div class="table-header">
        <div class="search-input">
          <span>🔍</span>
          <input v-model="searchQuery" type="text" placeholder="Search specializations..." />
        </div>
      </div>

      <div v-if="loading" class="loading-state">Loading specializations...</div>
      <div v-else-if="error" class="error-state">{{ error }}</div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Consultation Cost</th>
            <th>Description</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredItems.length === 0">
            <td colspan="5" class="empty-state">No specializations found</td>
          </tr>
          <tr v-for="item in filteredItems" :key="item.id">
            <td>{{ item.id }}</td>
            <td>{{ item.name }}</td>
            <td>${{ (item.consultationCost || 0).toFixed(2) }}</td>
            <td>{{ item.description || '-' }}</td>
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
          <h2>{{ isEditing ? 'Edit' : 'Add' }} Specialization</h2>
          <button class="btn-close" @click="showModal = false">×</button>
        </div>
        <form @submit.prevent="handleSubmit" class="modal-body">
          <div class="form-group">
            <label>Name *</label>
            <input v-model="formData.name" type="text" required />
          </div>
          <div class="form-group">
            <label>Consultation Cost ($) *</label>
            <input v-model.number="formData.consultationCost" type="number" step="0.01" min="0" required />
          </div>
          <div class="form-group">
            <label>Description</label>
            <textarea v-model="formData.description" rows="3"></textarea>
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
.form-group input, .form-group textarea { padding: var(--spacing-sm); border: 1px solid #E5E7EB; border-radius: var(--radius-sm); font-size: 0.875rem; }
</style>
