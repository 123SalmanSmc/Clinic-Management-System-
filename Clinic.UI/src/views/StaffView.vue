<script setup>
import { ref, onMounted, computed } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { staffApi, specializationsApi } from '../services/api'

const staffList = ref([])
const specializations = ref([])
const staffTypes = ref([])
const loading = ref(true)
const error = ref(null)
const searchQuery = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const selectedStaff = ref(null)

const formData = ref({
  fullName: '',
  phoneNumber: '',
  gender: 'Male',
  email: '',
  dateOfBirth: '',
  status: 'Active',
  fee: 0,
  feeType: 'Per Session',
  staffTypeId: 1,
  specializationId: null
})

const filteredStaff = computed(() => {
  if (!searchQuery.value) return staffList.value
  const query = searchQuery.value.toLowerCase()
  return staffList.value.filter(s => 
    s.fullName.toLowerCase().includes(query) ||
    (s.specializationName?.toLowerCase().includes(query))
  )
})

onMounted(async () => {
  await fetchData()
})

const fetchData = async () => {
  loading.value = true
  error.value = null
  try {
    const [staffRes, specRes, typesRes] = await Promise.all([
      staffApi.getAll(),
      specializationsApi.getAll().catch(() => ({ data: [] })),
      staffApi.getTypes().catch(() => ({ data: [] }))
    ])
    staffList.value = staffRes.data
    specializations.value = specRes.data
    staffTypes.value = typesRes.data
  } catch (e) {
    error.value = 'Failed to load staff'
    console.error(e)
  } finally {
    loading.value = false
  }
}

const openAddModal = () => {
  isEditing.value = false
  formData.value = { fullName: '', phoneNumber: '', gender: 'Male', email: '', dateOfBirth: '', status: 'Active', fee: 0, feeType: 'Per Session', staffTypeId: 1, specializationId: null }
  showModal.value = true
}

const openEditModal = (staff) => {
  isEditing.value = true
  selectedStaff.value = staff
  formData.value = {
    fullName: staff.fullName,
    phoneNumber: staff.phoneNumber,
    gender: staff.gender,
    email: staff.email || '',
    dateOfBirth: staff.dateOfBirth?.split('T')[0] || '',
    status: staff.status,
    fee: staff.fee,
    feeType: staff.feeType,
    staffTypeId: staff.staffTypeId,
    specializationId: staff.specializationId
  }
  showModal.value = true
}

const handleSubmit = async () => {
  try {
    const staffData = {
      ...formData.value,
      dateOfBirth: formData.value.dateOfBirth ? new Date(formData.value.dateOfBirth).toISOString() : null,
      fee: parseFloat(formData.value.fee) || 0,
    }
    if (isEditing.value) {
      await staffApi.update(selectedStaff.value.id, staffData)
    } else {
      await staffApi.create(staffData)
    }
    showModal.value = false
    await fetchData()
  } catch (e) {
    console.error('Failed to save staff:', e)
  }
}

const handleDelete = async (staff) => {
  if (!confirm(`Are you sure you want to delete "${staff.fullName}"?`)) return
  try {
    await staffApi.delete(staff.id)
    await fetchData()
  } catch (e) {
    console.error('Failed to delete staff:', e)
  }
}

const formatCurrency = (amount) => {
  return '$' + (amount || 0).toFixed(2)
}
</script>

<template>
  <MainLayout>
    <div class="page-header">
      <h1>Staff</h1>
      <button class="btn btn-primary" @click="openAddModal">
        + Add Staff
      </button>
    </div>

    <div class="card">
      <div class="table-header">
        <div class="search-input">
          <span>🔍</span>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Search staff..."
          />
        </div>
      </div>

      <div v-if="loading" class="loading-state">
        Loading staff...
      </div>

      <div v-else-if="error" class="error-state">
        {{ error }}
        <button @click="fetchData" class="btn btn-secondary">Retry</button>
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Full Name</th>
            <th>Phone</th>
            <th>Role</th>
            <th>Specialization</th>
            <th>Status</th>
            <th>Fee</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredStaff.length === 0">
            <td colspan="8" class="empty-state">No staff found</td>
          </tr>
          <tr v-for="staff in filteredStaff" :key="staff.id">
            <td>{{ staff.id }}</td>
            <td>{{ staff.fullName }}</td>
            <td>{{ staff.phoneNumber || '-' }}</td>
            <td>{{ staff.staffTypeName || '-' }}</td>
            <td>{{ staff.specializationName || '-' }}</td>
            <td>
              <span :class="['status-badge', staff.status?.toLowerCase()]">
                {{ staff.status }}
              </span>
            </td>
            <td>{{ formatCurrency(staff.fee) }}</td>
            <td>
              <button class="btn-icon" title="Edit" @click="openEditModal(staff)">✏️</button>
              <button class="btn-icon" title="Delete" @click="handleDelete(staff)">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Add Staff Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Edit' : 'Add' }} Staff</h2>
          <button class="btn-close" @click="showModal = false">×</button>
        </div>
        <form @submit.prevent="handleSubmit" class="modal-body">
          <div class="form-row">
            <div class="form-group">
              <label>Full Name *</label>
              <input v-model="formData.fullName" type="text" required />
            </div>
            <div class="form-group">
              <label>Phone Number</label>
              <input v-model="formData.phoneNumber" type="text" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Gender *</label>
              <select v-model="formData.gender" required>
                <option value="Male">Male</option>
                <option value="Female">Female</option>
              </select>
            </div>
            <div class="form-group">
              <label>Email</label>
              <input v-model="formData.email" type="email" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Staff Type *</label>
              <select v-model="formData.staffTypeId" required>
                <option v-for="type in staffTypes" :key="type.id" :value="type.id">
                  {{ type.name }}
                </option>
              </select>
            </div>
            <div class="form-group">
              <label>Specialization</label>
              <select v-model="formData.specializationId">
                <option :value="null">None</option>
                <option v-for="spec in specializations" :key="spec.id" :value="spec.id">
                  {{ spec.name }}
                </option>
              </select>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Fee</label>
              <input v-model="formData.fee" type="number" step="0.01" />
            </div>
            <div class="form-group">
              <label>Fee Type</label>
              <select v-model="formData.feeType">
                <option value="Per Session">Per Session</option>
                <option value="Monthly">Monthly</option>
              </select>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showAddModal = false">Cancel</button>
            <button type="submit" class="btn btn-primary">Save Staff</button>
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

.table-header {
  margin-bottom: var(--spacing-md);
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

.status-badge {
  padding: 4px 8px;
  border-radius: var(--radius-sm);
  font-size: 0.75rem;
  font-weight: 500;
}

.status-badge.active {
  background-color: #D1FAE5;
  color: #065F46;
}

.status-badge.inactive {
  background-color: #FEE2E2;
  color: #991B1B;
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
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
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

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-md);
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
.form-group select,
.form-group textarea {
  padding: var(--spacing-sm);
  border: 1px solid #E5E7EB;
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: var(--color-primary);
}
</style>
