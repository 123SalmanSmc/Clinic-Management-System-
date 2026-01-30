<script setup>
import { ref, onMounted, computed } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { patientsApi } from '../services/api'

const patients = ref([])
const loading = ref(true)
const error = ref(null)
const searchQuery = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const selectedPatient = ref(null)

// Form data
const formData = ref({
  fullName: '',
  phoneNumber: '',
  dateOfBirth: '',
  gender: 'Male',
  bloodType: '',
  email: '',
  weight: null,
  height: null,
  otherMedicalNotes: ''
})

const filteredPatients = computed(() => {
  if (!searchQuery.value) return patients.value
  const query = searchQuery.value.toLowerCase()
  return patients.value.filter(p => 
    p.fullName.toLowerCase().includes(query) ||
    p.phoneNumber.includes(query)
  )
})

onMounted(async () => {
  await fetchPatients()
})

const fetchPatients = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await patientsApi.getAll()
    patients.value = response.data
  } catch (e) {
    error.value = 'Failed to load patients'
    console.error(e)
  } finally {
    loading.value = false
  }
}

const openAddModal = () => {
  isEditing.value = false
  resetForm()
  showModal.value = true
}

const openEditModal = (patient) => {
  isEditing.value = true
  selectedPatient.value = patient
  formData.value = {
    fullName: patient.fullName,
    phoneNumber: patient.phoneNumber,
    dateOfBirth: patient.dateOfBirth?.split('T')[0] || '',
    gender: patient.gender,
    bloodType: patient.bloodType || '',
    email: patient.email || '',
    weight: patient.weight,
    height: patient.height,
    otherMedicalNotes: patient.otherMedicalNotes || ''
  }
  showModal.value = true
}

const handleSubmit = async () => {
  try {
    const patientData = {
      ...formData.value,
      dateOfBirth: new Date(formData.value.dateOfBirth).toISOString(),
      weight: formData.value.weight ? parseFloat(formData.value.weight) : null,
      height: formData.value.height ? parseFloat(formData.value.height) : null
    }
    
    if (isEditing.value) {
      await patientsApi.update(selectedPatient.value.id, patientData)
    } else {
      await patientsApi.create(patientData)
    }
    showModal.value = false
    resetForm()
    await fetchPatients()
  } catch (e) {
    console.error('Failed to save patient:', e)
  }
}

const handleDelete = async (patient) => {
  if (!confirm(`Are you sure you want to delete "${patient.fullName}"?`)) return
  try {
    await patientsApi.delete(patient.id)
    await fetchPatients()
  } catch (e) {
    console.error('Failed to delete patient:', e)
  }
}

const resetForm = () => {
  formData.value = {
    fullName: '',
    phoneNumber: '',
    dateOfBirth: '',
    gender: 'Male',
    bloodType: '',
    email: '',
    weight: null,
    height: null,
    otherMedicalNotes: ''
  }
}

const formatDate = (dateStr) => {
  return new Date(dateStr).toLocaleDateString()
}
</script>

<template>
  <MainLayout>
    <div class="page-header">
      <h1>Patients</h1>
      <button class="btn btn-primary" @click="openAddModal">
        + Add Patient
      </button>
    </div>

    <div class="card">
      <div class="table-header">
        <div class="search-input">
          <span>🔍</span>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Search patients..."
          />
        </div>
      </div>

      <div v-if="loading" class="loading-state">
        Loading patients...
      </div>

      <div v-else-if="error" class="error-state">
        {{ error }}
        <button @click="fetchPatients" class="btn btn-secondary">Retry</button>
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Full Name</th>
            <th>Phone</th>
            <th>Date of Birth</th>
            <th>Gender</th>
            <th>Blood Type</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredPatients.length === 0">
            <td colspan="7" class="empty-state">No patients found</td>
          </tr>
          <tr v-for="patient in filteredPatients" :key="patient.id">
            <td>{{ patient.id }}</td>
            <td>{{ patient.fullName }}</td>
            <td>{{ patient.phoneNumber }}</td>
            <td>{{ formatDate(patient.dateOfBirth) }}</td>
            <td>{{ patient.gender }}</td>
            <td>{{ patient.bloodType || '-' }}</td>
            <td>
              <button class="btn-icon" title="Edit" @click="openEditModal(patient)">✏️</button>
              <button class="btn-icon" title="Delete" @click="handleDelete(patient)">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Add Patient Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Edit' : 'Add' }} Patient</h2>
          <button class="btn-close" @click="showModal = false">×</button>
        </div>
        <form @submit.prevent="handleSubmit" class="modal-body">
          <div class="form-row">
            <div class="form-group">
              <label>Full Name *</label>
              <input v-model="formData.fullName" type="text" required />
            </div>
            <div class="form-group">
              <label>Phone Number *</label>
              <input v-model="formData.phoneNumber" type="text" required />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Date of Birth *</label>
              <input v-model="formData.dateOfBirth" type="date" required />
            </div>
            <div class="form-group">
              <label>Gender *</label>
              <select v-model="formData.gender" required>
                <option value="Male">Male</option>
                <option value="Female">Female</option>
              </select>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Blood Type</label>
              <select v-model="formData.bloodType">
                <option value="">Select</option>
                <option value="A+">A+</option>
                <option value="A-">A-</option>
                <option value="B+">B+</option>
                <option value="B-">B-</option>
                <option value="AB+">AB+</option>
                <option value="AB-">AB-</option>
                <option value="O+">O+</option>
                <option value="O-">O-</option>
              </select>
            </div>
            <div class="form-group">
              <label>Email</label>
              <input v-model="formData.email" type="email" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Weight (kg)</label>
              <input v-model="formData.weight" type="number" step="0.1" />
            </div>
            <div class="form-group">
              <label>Height (cm)</label>
              <input v-model="formData.height" type="number" step="0.1" />
            </div>
          </div>
          <div class="form-group">
            <label>Medical Notes</label>
            <textarea v-model="formData.otherMedicalNotes" rows="3"></textarea>
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
