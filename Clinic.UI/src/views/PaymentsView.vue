<script setup>
import { ref, onMounted, computed } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { paymentsApi, appointmentsApi } from '../services/api'

const payments = ref([])
const appointments = ref([])
const loading = ref(true)
const error = ref(null)
const searchQuery = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const selectedItem = ref(null)

const formData = ref({
  appointmentId: null,
  amount: 0,
  paymentMethod: 'Cash',
  paymentDate: new Date().toISOString().split('T')[0]
})

const filteredItems = computed(() => {
  if (!searchQuery.value) return payments.value
  const query = searchQuery.value.toLowerCase()
  return payments.value.filter(p => 
    p.patientName?.toLowerCase().includes(query) || 
    p.paymentMethod?.toLowerCase().includes(query)
  )
})

onMounted(async () => {
  await fetchData()
})

const fetchData = async () => {
  loading.value = true
  error.value = null
  try {
    const [paymentsRes, appointmentsRes] = await Promise.all([
      paymentsApi.getAll(),
      appointmentsApi.getAll().catch(() => ({ data: [] }))
    ])
    payments.value = paymentsRes.data
    appointments.value = appointmentsRes.data
  } catch (e) {
    error.value = 'Failed to load payments'
    console.error(e)
  } finally {
    loading.value = false
  }
}

const openAddModal = () => {
  isEditing.value = false
  formData.value = { 
    appointmentId: appointments.value[0]?.id || null, 
    amount: 0, 
    paymentMethod: 'Cash',
    paymentDate: new Date().toISOString().split('T')[0]
  }
  showModal.value = true
}

const openEditModal = (item) => {
  isEditing.value = true
  selectedItem.value = item
  formData.value = { 
    appointmentId: item.appointmentId, 
    amount: item.amount, 
    paymentMethod: item.paymentMethod,
    paymentDate: item.paymentDate?.split('T')[0] || new Date().toISOString().split('T')[0]
  }
  showModal.value = true
}

const handleSubmit = async () => {
  try {
    const data = { 
      ...formData.value, 
      amount: parseFloat(formData.value.amount),
      paymentDate: new Date(formData.value.paymentDate).toISOString()
    }
    if (isEditing.value) {
      await paymentsApi.update(selectedItem.value.id, data)
    } else {
      await paymentsApi.create(data)
    }
    showModal.value = false
    await fetchData()
  } catch (e) {
    console.error('Failed to save payment:', e)
  }
}

const handleDelete = async (item) => {
  if (!confirm(`Are you sure you want to delete this payment?`)) return
  try {
    await paymentsApi.delete(item.id)
    await fetchData()
  } catch (e) {
    console.error('Failed to delete payment:', e)
  }
}

const formatCurrency = (amount) => '$' + (amount || 0).toFixed(2)
const formatDate = (dateStr) => dateStr ? new Date(dateStr).toLocaleDateString() : '-'
</script>

<template>
  <MainLayout>
    <div class="page-header">
      <h1>Payments</h1>
      <button class="btn btn-primary" @click="openAddModal">+ Add Payment</button>
    </div>

    <div class="card">
      <div class="table-header">
        <div class="search-input">
          <span>🔍</span>
          <input v-model="searchQuery" type="text" placeholder="Search payments..." />
        </div>
      </div>

      <div v-if="loading" class="loading-state">Loading payments...</div>
      <div v-else-if="error" class="error-state">{{ error }}</div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Patient</th>
            <th>Appointment</th>
            <th>Amount</th>
            <th>Method</th>
            <th>Date</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredItems.length === 0">
            <td colspan="7" class="empty-state">No payments found</td>
          </tr>
          <tr v-for="item in filteredItems" :key="item.id">
            <td>{{ item.id }}</td>
            <td>{{ item.patientName || '-' }}</td>
            <td>#{{ item.appointmentId }}</td>
            <td>{{ formatCurrency(item.amount) }}</td>
            <td>
              <span :class="['method-badge', item.paymentMethod?.toLowerCase()]">
                {{ item.paymentMethod }}
              </span>
            </td>
            <td>{{ formatDate(item.paymentDate) }}</td>
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
          <h2>{{ isEditing ? 'Edit' : 'Add' }} Payment</h2>
          <button class="btn-close" @click="showModal = false">×</button>
        </div>
        <form @submit.prevent="handleSubmit" class="modal-body">
          <div class="form-group">
            <label>Appointment *</label>
            <select v-model="formData.appointmentId" required>
              <option v-for="apt in appointments" :key="apt.id" :value="apt.id">
                #{{ apt.id }} - {{ apt.patientName }} ({{ formatDate(apt.scheduleDate) }})
              </option>
            </select>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Amount *</label>
              <input v-model="formData.amount" type="number" step="0.01" required />
            </div>
            <div class="form-group">
              <label>Method *</label>
              <select v-model="formData.paymentMethod" required>
                <option value="Cash">Cash</option>
                <option value="Card">Card</option>
                <option value="Bank Transfer">Bank Transfer</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>
          <div class="form-group">
            <label>Payment Date *</label>
            <input v-model="formData.paymentDate" type="date" required />
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
.method-badge { padding: 4px 8px; border-radius: var(--radius-sm); font-size: 0.75rem; background-color: #E0E7FF; color: #3730A3; }
.method-badge.cash { background-color: #D1FAE5; color: #065F46; }
.method-badge.card { background-color: #DBEAFE; color: #1E40AF; }
.btn-icon { background: transparent; border: none; cursor: pointer; padding: var(--spacing-xs); font-size: 1rem; }
.loading-state, .error-state, .empty-state { text-align: center; padding: var(--spacing-xl); color: var(--color-text-muted); }
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background-color: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.modal { background-color: var(--color-bg-card); border-radius: var(--radius-lg); width: 100%; max-width: 500px; }
.modal-header { display: flex; justify-content: space-between; align-items: center; padding: var(--spacing-lg); border-bottom: 1px solid #E5E7EB; }
.btn-close { background: transparent; border: none; font-size: 1.5rem; cursor: pointer; color: var(--color-text-muted); }
.modal-body { padding: var(--spacing-lg); }
.modal-footer { display: flex; justify-content: flex-end; gap: var(--spacing-sm); margin-top: var(--spacing-lg); }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--spacing-md); }
.form-group { display: flex; flex-direction: column; gap: var(--spacing-xs); margin-bottom: var(--spacing-md); }
.form-group label { font-size: 0.875rem; font-weight: 500; }
.form-group input, .form-group select { padding: var(--spacing-sm); border: 1px solid #E5E7EB; border-radius: var(--radius-sm); font-size: 0.875rem; }
</style>
