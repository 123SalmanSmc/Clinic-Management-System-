<script setup>
import { ref, onMounted, computed } from 'vue'
import MainLayout from '../layouts/MainLayout.vue'
import { appointmentsApi, patientsApi, staffApi, servicesApi, serviceTypesApi, taxApi, serviceAssignmentsApi, medicalNotesApi } from '../services/api'

// Data lists
const appointments = ref([])
const patients = ref([])
const doctors = ref([])
const services = ref([])
const serviceTypes = ref([])
const activeVatRate = ref(0)

// UI State
const loading = ref(true)
const error = ref(null)
const searchQuery = ref('')
const showModal = ref(false)
const showServiceModal = ref(false)
const showNotesModal = ref(false)
const isEditing = ref(false)
const selectedAppointment = ref(null)
const selectedPatient = ref(null)

// Appointment Form Data
const formData = ref({
  patientId: null,
  doctorId: null,
  scheduleDate: '',
  scheduleTime: '',
  discount: 0,
  payingAmount: 0
})

// Computed Helpers for Appointment Form
const selectedDoctor = computed(() => doctors.value.find(d => d.id === formData.value.doctorId))
const consultantFee = computed(() => selectedDoctor.value?.specializationCost || selectedDoctor.value?.fee || 0)
const apptVatAmount = computed(() => consultantFee.value * (activeVatRate.value / 100))
const apptGrandTotal = computed(() => consultantFee.value + apptVatAmount.value - (formData.value.discount || 0))
const apptBalance = computed(() => apptGrandTotal.value - (formData.value.payingAmount || 0))

// Service Assignment Data
const serviceFormData = ref({ selectedServiceId: null, selectedServiceTypeId: null })
const serviceItems = ref([])
const serviceDiscount = ref(0)
const servicePayingAmount = ref(0)
const existingAssignments = ref([]) // List of existing service assignments

// Computed Helpers for Service Modal
const filteredServiceTypes = computed(() => {
  if (!serviceFormData.value.selectedServiceId) return []
  return serviceTypes.value.filter(st => st.serviceId === serviceFormData.value.selectedServiceId)
})
const selectedServiceType = computed(() => serviceTypes.value.find(st => st.id === serviceFormData.value.selectedServiceTypeId))
const serviceTotalCost = computed(() => serviceItems.value.reduce((sum, item) => sum + item.cost, 0))
const serviceVatAmount = computed(() => serviceTotalCost.value * (activeVatRate.value / 100))
const serviceGrandTotal = computed(() => serviceTotalCost.value + serviceVatAmount.value - serviceDiscount.value)
const serviceBalance = computed(() => serviceGrandTotal.value - servicePayingAmount.value)

// Medical Notes Data
const medicalNotes = ref([])
const noteFormData = ref({
  diagnosis: '',
  symptoms: '',
  treatment: '',
  prescription: '',
  notes: ''
})

const filteredAppointments = computed(() => {
  if (!searchQuery.value) return appointments.value
  const query = searchQuery.value.toLowerCase()
  return appointments.value.filter(a => 
    a.patientName?.toLowerCase().includes(query) ||
    a.doctorName?.toLowerCase().includes(query)
  )
})

onMounted(async () => await fetchData())

const fetchData = async () => {
  loading.value = true
  error.value = null
  try {
    const [apptRes, patientsRes, doctorsRes, servicesRes, serviceTypesRes, taxRes] = await Promise.all([
      appointmentsApi.getAll(),
      patientsApi.getAll().catch(() => ({ data: [] })),
      staffApi.getDoctors().catch(() => ({ data: [] })),
      servicesApi.getAll().catch(() => ({ data: [] })),
      serviceTypesApi.getAll().catch(() => ({ data: [] })),
      taxApi.getAll().catch(() => ({ data: [] }))
    ])
    appointments.value = apptRes.data
    patients.value = patientsRes.data
    doctors.value = doctorsRes.data
    services.value = servicesRes.data
    serviceTypes.value = serviceTypesRes.data
    
    // Get VAT Ratio from active VAT taxes (Category='VAT', Status=1)
    const vatTaxes = taxRes.data.filter(t => t.category === 'VAT' && t.status === 1)
    activeVatRate.value = vatTaxes.reduce((sum, t) => sum + (t.ratio || 0), 0)
  } catch (e) {
    error.value = 'Failed to load data'
    console.error(e)
  } finally {
    loading.value = false
  }
}

// --- Appointment Modal ---
const openAddModal = () => {
  isEditing.value = false
  formData.value = { patientId: null, doctorId: null, scheduleDate: new Date().toISOString().split('T')[0], scheduleTime: '', discount: 0, payingAmount: 0 }
  showModal.value = true
}

const openEditModal = (appt) => {
  isEditing.value = true
  selectedAppointment.value = appt
  formData.value = {
    patientId: appt.patientId,
    doctorId: appt.doctorId,
    scheduleDate: appt.scheduleDate?.split('T')[0] || '',
    scheduleTime: appt.scheduleTime?.substring(0, 5) || '',
    discount: appt.discount,
    payingAmount: appt.payingAmount
  }
  showModal.value = true
}

const handleSubmit = async () => {
  try {
    const data = {
      patientId: formData.value.patientId,
      doctorId: formData.value.doctorId,
      scheduleDate: new Date(formData.value.scheduleDate).toISOString(),
      scheduleTime: formData.value.scheduleTime + ':00',
      consultationCost: consultantFee.value,
      discount: parseFloat(formData.value.discount) || 0,
      vat: apptVatAmount.value,
      payingAmount: parseFloat(formData.value.payingAmount) || 0
    }
    if (isEditing.value) {
      await appointmentsApi.update(selectedAppointment.value.id, data)
    } else {
      await appointmentsApi.create(data)
    }
    showModal.value = false
    await fetchData()
  } catch (e) {
    console.error('Failed to save appointment:', e)
    alert(e.response?.data || 'Failed to save appointment')
  }
}

// --- Service Assignment Modal ---
const openServiceModal = async (appt) => {
  selectedAppointment.value = appt
  selectedPatient.value = patients.value.find(p => p.id === appt.patientId)
  serviceFormData.value = { selectedServiceId: null, selectedServiceTypeId: null }
  serviceItems.value = []
  serviceDiscount.value = 0
  servicePayingAmount.value = 0
  existingAssignments.value = []
  showServiceModal.value = true
  
  // Fetch existing assignments
  try {
    const res = await serviceAssignmentsApi.getByAppointment(appt.id)
    existingAssignments.value = res.data
  } catch (e) {
    console.error('Failed to load existing assignments:', e)
  }
}

const addServiceToTable = () => {
  if (!selectedServiceType.value) return
  if (serviceItems.value.some(item => item.id === selectedServiceType.value.id)) {
    alert('Service already added')
    return
  }
  serviceItems.value.push({ id: selectedServiceType.value.id, name: selectedServiceType.value.serviceTypeName, cost: selectedServiceType.value.cost })
  serviceFormData.value.selectedServiceTypeId = null
}

const removeServiceFromTable = (id) => {
  serviceItems.value = serviceItems.value.filter(item => item.id !== id)
}

const handleServiceSubmit = async () => {
  if (serviceItems.value.length === 0) {
    alert('Please add at least one service')
    return
  }
  try {
    await serviceAssignmentsApi.create({
      appointmentId: selectedAppointment.value.id,
      serviceTypeIds: serviceItems.value.map(item => item.id),
      discount: parseFloat(serviceDiscount.value) || 0,
      payingAmount: parseFloat(servicePayingAmount.value) || 0
    })
    // Refresh list
    const res = await serviceAssignmentsApi.getByAppointment(selectedAppointment.value.id)
    existingAssignments.value = res.data
    serviceItems.value = []
    serviceDiscount.value = 0
    servicePayingAmount.value = 0
    alert('Services assigned successfully!')
  } catch (e) {
    console.error('Failed to assign services:', e)
    alert(e.response?.data || 'Failed to assign services')
  }
}

const deleteAssignment = async (id) => {
  if (!confirm('Delete this service assignment?')) return
  try {
    await serviceAssignmentsApi.delete(id)
    existingAssignments.value = existingAssignments.value.filter(a => a.id !== id)
  } catch (e) {
    console.error('Failed to delete assignment:', e)
  }
}

// --- Medical Notes Modal ---
const openNotesModal = async (appt) => {
  selectedAppointment.value = appt
  selectedPatient.value = patients.value.find(p => p.id === appt.patientId)
  noteFormData.value = { diagnosis: '', symptoms: '', treatment: '', prescription: '', notes: '' }
  showNotesModal.value = true
  medicalNotes.value = []
  
  try {
    const res = await medicalNotesApi.getByAppointment(appt.id)
    medicalNotes.value = res.data
  } catch (e) {
    console.error('Failed to load medical notes:', e)
  }
}

const handleAddNote = async () => {
  if (!noteFormData.value.diagnosis && !noteFormData.value.notes) {
    alert('Please provide a diagnosis or notes')
    return
  }
  try {
    await medicalNotesApi.create({ appointmentId: selectedAppointment.value.id, ...noteFormData.value })
    const res = await medicalNotesApi.getByAppointment(selectedAppointment.value.id)
    medicalNotes.value = res.data
    noteFormData.value = { diagnosis: '', symptoms: '', treatment: '', prescription: '', notes: '' }
  } catch (e) {
    console.error('Failed to add note:', e)
    alert('Failed to add note')
  }
}

const deleteNote = async (id) => {
  if (!confirm('Delete this medical note?')) return
  try {
    await medicalNotesApi.delete(id)
    medicalNotes.value = medicalNotes.value.filter(n => n.id !== id)
  } catch (e) {
    console.error('Failed to delete note:', e)
  }
}

const handleDelete = async (appt) => {
  if (!confirm(`Delete appointment #${appt.id}?`)) return
  try {
    await appointmentsApi.delete(appt.id)
    await fetchData()
  } catch (e) {
    console.error('Failed to delete appointment:', e)
  }
}

const formatDate = (dateStr) => new Date(dateStr).toLocaleDateString()
const formatCurrency = (amount) => '$' + (amount || 0).toFixed(2)
</script>

<template>
  <MainLayout>
    <div class="page-header">
      <h1>Appointments</h1>
      <button class="btn btn-primary" @click="openAddModal">+ New Appointment</button>
    </div>

    <div class="card">
      <div class="table-header">
        <div class="search-input">
          <span>🔍</span>
          <input v-model="searchQuery" type="text" placeholder="Search appointments..." />
        </div>
      </div>

      <div v-if="loading" class="loading-state">Loading appointments...</div>
      <div v-else-if="error" class="error-state">{{ error }} <button @click="fetchData" class="btn btn-secondary">Retry</button></div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Patient</th>
            <th>Doctor</th>
            <th>Date</th>
            <th>Time</th>
            <th>Total</th>
            <th>Paid</th>
            <th>Balance</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredAppointments.length === 0">
            <td colspan="9" class="empty-state">No appointments found</td>
          </tr>
          <tr v-for="appt in filteredAppointments" :key="appt.id">
            <td>{{ appt.id }}</td>
            <td>{{ appt.patientName }}</td>
            <td>{{ appt.doctorName }}</td>
            <td>{{ formatDate(appt.scheduleDate) }}</td>
            <td>{{ appt.scheduleTime || '-' }}</td>
            <td>{{ formatCurrency(appt.grandTotal) }}</td>
            <td>{{ formatCurrency(appt.payingAmount) }}</td>
            <td>
              <span :class="['balance-badge', appt.balance > 0 ? 'due' : 'paid']">
                {{ formatCurrency(appt.balance) }}
              </span>
            </td>
            <td>
              <button class="btn-icon" title="Assign Services" @click="openServiceModal(appt)">🩺</button>
              <button class="btn-icon" title="Medical Notes" @click="openNotesModal(appt)">📝</button>
              <button class="btn-icon" title="Edit" @click="openEditModal(appt)">✏️</button>
              <button class="btn-icon" title="Delete" @click="handleDelete(appt)">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Appointment Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal invoice-modal">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Edit' : 'New' }} Appointment</h2>
          <button class="btn-close" @click="showModal = false">×</button>
        </div>
        <form @submit.prevent="handleSubmit" class="modal-body">
          <div class="invoice-section">
            <h3 class="section-title">Patient & Doctor</h3>
            <div class="form-row">
              <div class="form-group">
                <label>Patient *</label>
                <select v-model="formData.patientId" required>
                  <option :value="null">Select Patient</option>
                  <option v-for="p in patients" :key="p.id" :value="p.id">{{ p.fullName }}</option>
                </select>
              </div>
              <div class="form-group">
                <label>Doctor *</label>
                <select v-model="formData.doctorId" required>
                  <option :value="null">Select Doctor</option>
                  <option v-for="d in doctors" :key="d.id" :value="d.id">
                    {{ d.fullName }} - {{ formatCurrency(d.specializationCost || d.fee || 0) }}
                  </option>
                </select>
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Date *</label>
                <input v-model="formData.scheduleDate" type="date" required />
              </div>
              <div class="form-group">
                <label>Time *</label>
                <input v-model="formData.scheduleTime" type="time" required />
              </div>
            </div>
          </div>

          <div class="invoice-section">
            <h3 class="section-title">Financials</h3>
            <table class="invoice-table">
              <tbody>
                <tr><td>Consultation Fee</td><td class="amount-col">{{ formatCurrency(consultantFee) }}</td></tr>
                <tr><td>VAT ({{ activeVatRate }}%)</td><td class="amount-col">{{ formatCurrency(apptVatAmount) }}</td></tr>
                <tr>
                  <td>Discount</td>
                  <td class="amount-col"><input v-model.number="formData.discount" type="number" step="0.01" min="0" class="inline-input" /></td>
                </tr>
                <tr class="total-row"><td>Grand Total</td><td class="amount-col">{{ formatCurrency(apptGrandTotal) }}</td></tr>
                <tr>
                  <td>Paying Amount</td>
                  <td class="amount-col"><input v-model.number="formData.payingAmount" type="number" step="0.01" min="0" class="inline-input" /></td>
                </tr>
                <tr class="balance-row">
                  <td>Balance Due</td>
                  <td :class="['amount-col', apptBalance > 0 ? 'text-danger' : 'text-success']">{{ formatCurrency(apptBalance) }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showModal = false">Cancel</button>
            <button type="submit" class="btn btn-primary">{{ isEditing ? 'Update' : 'Confirm' }}</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Service Assignment Modal -->
    <div v-if="showServiceModal" class="modal-overlay" @click.self="showServiceModal = false">
      <div class="modal invoice-modal modal-wide">
        <div class="modal-header">
          <h2>Assign Services</h2>
          <button class="btn-close" @click="showServiceModal = false">×</button>
        </div>
        <div class="modal-body">
          <div class="invoice-header-info">
            <div class="info-group"><label>Patient:</label><div class="info-value">{{ selectedPatient?.fullName || 'Unknown' }}</div></div>
            <div class="info-group"><label>Appointment #:</label><div class="info-value">{{ selectedAppointment?.id }}</div></div>
          </div>

          <!-- Existing Assignments -->
          <div v-if="existingAssignments.length > 0" class="existing-section">
            <h4>Existing Service Assignments</h4>
            <table class="items-table">
              <thead><tr><th>ID</th><th>Services</th><th>Total</th><th>VAT</th><th>Grand Total</th><th></th></tr></thead>
              <tbody>
                <tr v-for="assign in existingAssignments" :key="assign.id">
                  <td>{{ assign.id }}</td>
                  <td>{{ assign.details.map(d => d.serviceTypeName).join(', ') }}</td>
                  <td>{{ formatCurrency(assign.totalCost) }}</td>
                  <td>{{ formatCurrency(assign.vat) }}</td>
                  <td>{{ formatCurrency(assign.grandTotal) }}</td>
                  <td><span class="remove-btn" @click="deleteAssignment(assign.id)">🗑️</span></td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Add New -->
          <div class="service-selector">
            <h4>Add New Services</h4>
            <div class="form-row">
              <div class="form-group">
                <select v-model="serviceFormData.selectedServiceId">
                  <option :value="null">Select Category</option>
                  <option v-for="s in services" :key="s.id" :value="s.id">{{ s.serviceName }}</option>
                </select>
              </div>
              <div class="form-group">
                <select v-model="serviceFormData.selectedServiceTypeId" :disabled="!serviceFormData.selectedServiceId">
                  <option :value="null">Select Service</option>
                  <option v-for="st in filteredServiceTypes" :key="st.id" :value="st.id">
                    {{ st.serviceTypeName }} - {{ formatCurrency(st.cost) }}
                  </option>
                </select>
              </div>
              <button type="button" class="btn btn-primary btn-sm" @click="addServiceToTable" :disabled="!serviceFormData.selectedServiceTypeId">Add</button>
            </div>
          </div>

          <table class="items-table">
            <thead><tr><th>Service</th><th class="text-right">Cost</th><th></th></tr></thead>
            <tbody>
              <tr v-if="serviceItems.length === 0"><td colspan="3" class="text-center text-muted">No items added</td></tr>
              <tr v-for="item in serviceItems" :key="item.id">
                <td>{{ item.name }}</td>
                <td class="text-right">{{ formatCurrency(item.cost) }}</td>
                <td class="text-center"><span class="remove-btn" @click="removeServiceFromTable(item.id)">×</span></td>
              </tr>
            </tbody>
          </table>

          <div class="invoice-totals">
            <table class="totals-table">
              <tr><td>Subtotal:</td><td>{{ formatCurrency(serviceTotalCost) }}</td></tr>
              <tr><td>VAT ({{ activeVatRate }}%):</td><td>{{ formatCurrency(serviceVatAmount) }}</td></tr>
              <tr><td>Discount:</td><td><input v-model.number="serviceDiscount" type="number" step="0.01" min="0" class="subtle-input" /></td></tr>
              <tr class="grand-total"><td>Total:</td><td>{{ formatCurrency(serviceGrandTotal) }}</td></tr>
              <tr><td>Paying:</td><td><input v-model.number="servicePayingAmount" type="number" step="0.01" min="0" class="subtle-input" /></td></tr>
              <tr class="balance-due"><td>Balance:</td><td :class="serviceBalance > 0 ? 'text-danger' : 'text-success'">{{ formatCurrency(serviceBalance) }}</td></tr>
            </table>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showServiceModal = false">Close</button>
            <button type="button" class="btn btn-primary" @click="handleServiceSubmit" :disabled="serviceItems.length === 0">Submit</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Medical Notes Modal -->
    <div v-if="showNotesModal" class="modal-overlay" @click.self="showNotesModal = false">
      <div class="modal invoice-modal modal-wide">
        <div class="modal-header">
          <h2>Medical Notes - {{ selectedPatient?.fullName }}</h2>
          <button class="btn-close" @click="showNotesModal = false">×</button>
        </div>
        <div class="modal-body">
          <!-- Existing Notes -->
          <div v-if="medicalNotes.length > 0" class="notes-list">
            <div v-for="note in medicalNotes" :key="note.id" class="note-card">
              <div class="note-header">
                <span class="note-date">{{ formatDate(note.createdAt) }}</span>
                <button class="btn-icon" @click="deleteNote(note.id)">🗑️</button>
              </div>
              <div class="note-body">
                <p v-if="note.diagnosis"><strong>Diagnosis:</strong> {{ note.diagnosis }}</p>
                <p v-if="note.symptoms"><strong>Symptoms:</strong> {{ note.symptoms }}</p>
                <p v-if="note.treatment"><strong>Treatment:</strong> {{ note.treatment }}</p>
                <p v-if="note.prescription"><strong>Prescription:</strong> {{ note.prescription }}</p>
                <p v-if="note.notes"><strong>Notes:</strong> {{ note.notes }}</p>
              </div>
            </div>
          </div>
          <div v-else class="empty-state-sm">No medical notes for this appointment.</div>

          <!-- Add Note Form -->
          <div class="add-note-form">
            <h4>Add Medical Note</h4>
            <div class="form-row">
              <div class="form-group"><label>Diagnosis</label><input v-model="noteFormData.diagnosis" type="text" placeholder="Enter diagnosis..." /></div>
              <div class="form-group"><label>Symptoms</label><input v-model="noteFormData.symptoms" type="text" placeholder="Enter symptoms..." /></div>
            </div>
            <div class="form-row">
              <div class="form-group"><label>Treatment</label><input v-model="noteFormData.treatment" type="text" placeholder="Treatment plan..." /></div>
              <div class="form-group"><label>Prescription</label><input v-model="noteFormData.prescription" type="text" placeholder="Medications..." /></div>
            </div>
            <div class="form-group"><label>Additional Notes</label><textarea v-model="noteFormData.notes" rows="2" placeholder="Other notes..."></textarea></div>
            <button type="button" class="btn btn-primary" @click="handleAddNote">Save Note</button>
          </div>
        </div>
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
.balance-badge { padding: 4px 8px; border-radius: var(--radius-sm); font-size: 0.75rem; font-weight: 500; }
.balance-badge.paid { background-color: #D1FAE5; color: #065F46; }
.balance-badge.due { background-color: #FEE2E2; color: #991B1B; }
.btn-icon { background: transparent; border: none; cursor: pointer; padding: var(--spacing-xs); font-size: 1rem; }
.loading-state, .error-state, .empty-state { text-align: center; padding: var(--spacing-xl); color: var(--color-text-muted); }
.empty-state-sm { text-align: center; color: var(--color-text-muted); padding: 16px; font-size: 0.875rem; }
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background-color: rgba(0, 0, 0, 0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.text-danger { color: #DC2626; }
.text-success { color: #059669; }
.text-muted { color: #6B7280; }
.text-right { text-align: right; }
.text-center { text-align: center; }

.invoice-modal { background-color: white; border-radius: 8px; width: 100%; max-width: 550px; max-height: 90vh; overflow-y: auto; box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1); }
.modal-wide { max-width: 800px; }
.modal-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 24px; border-bottom: 1px solid #E5E7EB; background-color: #F9FAFB; border-radius: 8px 8px 0 0; }
.modal-header h2 { font-size: 1.125rem; font-weight: 600; color: #111827; margin: 0; }
.btn-close { background: transparent; border: none; font-size: 1.5rem; cursor: pointer; color: #6B7280; }
.modal-body { padding: 24px; }
.modal-footer { margin-top: 20px; padding-top: 16px; border-top: 1px solid #E5E7EB; display: flex; justify-content: flex-end; gap: 12px; }

.invoice-section { margin-bottom: 20px; }
.section-title { font-size: 0.8rem; font-weight: 600; color: #374151; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 12px; border-bottom: 2px solid #E5E7EB; padding-bottom: 4px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 12px; align-items: end; }
.form-group { display: flex; flex-direction: column; gap: 4px; }
.form-group label { font-size: 0.75rem; font-weight: 500; color: #374151; }
.form-group input, .form-group select, .form-group textarea { padding: 8px 12px; border: 1px solid #D1D5DB; border-radius: 6px; font-size: 0.875rem; width: 100%; }
.form-group input:focus, .form-group select:focus, .form-group textarea:focus { outline: none; border-color: #3B82F6; box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1); }

.invoice-table { width: 100%; border-collapse: collapse; }
.invoice-table td { padding: 8px 0; border-bottom: 1px dashed #E5E7EB; font-size: 0.9rem; }
.invoice-table tr:last-child td { border-bottom: none; }
.amount-col { text-align: right; font-weight: 500; color: #111827; }
.inline-input { text-align: right; width: 80px; padding: 4px; border: 1px solid #D1D5DB; border-radius: 4px; font-size: 0.9rem; }
.total-row td { border-top: 2px solid #E5E7EB; font-weight: 700; font-size: 1rem; padding-top: 12px; }
.balance-row td { font-weight: 700; font-size: 1.05rem; padding-top: 8px; }

.invoice-header-info { display: flex; justify-content: space-between; background: #F3F4F6; padding: 12px 16px; border-radius: 6px; margin-bottom: 16px; }
.info-group { display: flex; flex-direction: column; }
.info-group label { font-size: 0.7rem; color: #6B7280; text-transform: uppercase; }
.info-value { font-weight: 600; color: #111827; font-size: 0.95rem; }

.existing-section { margin-bottom: 20px; }
.existing-section h4, .service-selector h4, .add-note-form h4 { font-size: 0.85rem; font-weight: 600; color: #374151; margin-bottom: 8px; }
.service-selector { background: #F9FAFB; padding: 16px; border-radius: 6px; border: 1px solid #E5E7EB; margin-bottom: 16px; }
.items-table { width: 100%; border-collapse: collapse; margin-bottom: 16px; }
.items-table th { text-align: left; font-size: 0.7rem; text-transform: uppercase; color: #6B7280; padding: 8px 4px; border-bottom: 2px solid #E5E7EB; }
.items-table td { padding: 10px 4px; border-bottom: 1px solid #E5E7EB; font-size: 0.875rem; color: #111827; }
.remove-btn { color: #EF4444; cursor: pointer; font-size: 1rem; }
.remove-btn:hover { color: #DC2626; }

.invoice-totals { display: flex; justify-content: flex-end; margin-bottom: 16px; }
.totals-table { width: 220px; }
.totals-table td { padding: 4px 0; text-align: right; font-size: 0.85rem; }
.totals-table td:first-child { color: #6B7280; padding-right: 12px; }
.totals-table td:last-child { font-weight: 500; color: #111827; }
.subtle-input { width: 70px; text-align: right; border: 1px solid #D1D5DB; border-radius: 4px; padding: 2px 4px; font-size: 0.85rem; }
.grand-total td { border-top: 2px solid #E5E7EB; padding-top: 10px; font-weight: 700; font-size: 1rem; color: #111827; }
.balance-due td { padding-top: 6px; font-weight: 700; font-size: 1rem; }

.notes-list { display: flex; flex-direction: column; gap: 12px; margin-bottom: 20px; max-height: 300px; overflow-y: auto; }
.note-card { background: #F9FAFB; border-radius: 8px; border: 1px solid #E5E7EB; padding: 12px 16px; }
.note-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
.note-date { font-size: 0.75rem; color: #6B7280; }
.note-body p { margin: 4px 0; font-size: 0.875rem; color: #374151; }
.note-body strong { color: #111827; }
.add-note-form { background: #F3F4F6; padding: 16px; border-radius: 8px; }
</style>
