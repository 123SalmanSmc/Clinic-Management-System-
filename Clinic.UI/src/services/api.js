import axios from 'axios'
import { useAuthStore } from '../stores/auth'
import router from '../router'

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080',
    headers: {
        'Content-Type': 'application/json'
    },
    withCredentials: true // Required for cookies to be sent
})

// Response interceptor - handle 401 errors
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            const authStore = useAuthStore()
            authStore.logout()
            router.push('/login')
        }
        return Promise.reject(error)
    }
)

// Auth endpoints
export const authApi = {
    login: (username, password) => api.post('/api/auth/login', { username, password }),
    register: (username, password, role) => api.post('/api/auth/register', { username, password, role }),
    // logout: () => api.post('/api/auth/logout'),
    me: () => api.get('/api/auth/me', { withCredentials: true })
}

// Dashboard endpoints
export const dashboardApi = {
    getStats: () => api.get('/api/dashboard/stats'),
    getTopSessions: () => api.get('/api/dashboard/top-sessions'),
    getSummary: () => api.get('/api/dashboard/summary')
}

// Patients endpoints
export const patientsApi = {
    getAll: () => api.get('/api/patients'),
    getById: (id) => api.get(`/api/patients/${id}`),
    create: (data) => api.post('/api/patients', data),
    update: (id, data) => api.put(`/api/patients/${id}`, data),
    delete: (id) => api.delete(`/api/patients/${id}`),
    getMedicalNotes: (id) => api.get(`/api/patients/${id}/medical-notes`),
    addMedicalNote: (id, note) => api.post(`/api/patients/${id}/medical-notes`, note)
}

// Staff endpoints
export const staffApi = {
    getAll: () => api.get('/api/staff'),
    getById: (id) => api.get(`/api/staff/${id}`),
    getDoctors: () => api.get('/api/staff/doctors'),
    getTypes: () => api.get('/api/staff/types'),
    createType: (data) => api.post('/api/staff/types', data),
    updateType: (id, data) => api.put(`/api/staff/types/${id}`, data),
    deleteType: (id) => api.delete(`/api/staff/types/${id}`),
    create: (data) => api.post('/api/staff', data),
    update: (id, data) => api.put(`/api/staff/${id}`, data),
    delete: (id) => api.delete(`/api/staff/${id}`)
}

// Appointments endpoints
export const appointmentsApi = {
    getAll: () => api.get('/api/appointments'),
    getById: (id) => api.get(`/api/appointments/${id}`),
    getToday: () => api.get('/api/appointments/today'),
    create: (data) => api.post('/api/appointments', data),
    submit: (data) => api.post('/api/appointments/submit', data),
    update: (id, data) => api.put(`/api/appointments/${id}`, data),
    delete: (id) => api.delete(`/api/appointments/${id}`)
}

// Services endpoints
export const servicesApi = {
    getAll: () => api.get('/api/services'),
    getById: (id) => api.get(`/api/services/${id}`),
    create: (data) => api.post('/api/services', data),
    update: (id, data) => api.put(`/api/services/${id}`, data),
    delete: (id) => api.delete(`/api/services/${id}`)
}

// Service Types endpoints
export const serviceTypesApi = {
    getAll: () => api.get('/api/servicetypes'),
    getById: (id) => api.get(`/api/servicetypes/${id}`),
    getByService: (serviceId) => api.get(`/api/servicetypes/by-service/${serviceId}`),
    create: (data) => api.post('/api/servicetypes', data),
    update: (id, data) => api.put(`/api/servicetypes/${id}`, data),
    delete: (id) => api.delete(`/api/servicetypes/${id}`)
}

// Specializations endpoints
export const specializationsApi = {
    getAll: () => api.get('/api/specializations', { withCredentials: true }),
    getById: (id) => api.get(`/api/specializations/${id}`),
    create: (data) => api.post('/api/specializations', data, { withCredentials: true }),
    update: (id, data) => api.put(`/api/specializations/${id}`, data),
    delete: (id) => api.delete(`/api/specializations/${id}`)
}

// Tax endpoints
export const taxApi = {
    getAll: () => api.get('/api/tax'),
    getDefault: () => api.get('/api/tax/default'),
    create: (data) => api.post('/api/tax', data),
    update: (id, data) => api.put(`/api/tax/${id}`, data),
    delete: (id) => api.delete(`/api/tax/${id}`)
}

// Payments endpoints
export const paymentsApi = {
    getAll: () => api.get('/api/payments'),
    getDues: () => api.get('/api/payments/dues'),
    create: (data) => api.post('/api/payments', data),
    update: (id, data) => api.put(`/api/payments/${id}`, data),
    delete: (id) => api.delete(`/api/payments/${id}`)
}

// Service Assignments endpoints
export const serviceAssignmentsApi = {
    create: (data) => api.post('/api/service-assignments', data),
    getByAppointment: (appointmentId) => api.get(`/api/service-assignments/by-appointment/${appointmentId}`),
    delete: (id) => api.delete(`/api/service-assignments/${id}`)
}

// Roles endpoints
export const rolesApi = {
    getAll: () => api.get('/api/roles'),
    get: (id) => api.get(`/api/roles/${id}`),
    create: (data) => api.post('/api/roles', data),
    update: (id, data) => api.put(`/api/roles/${id}`, data),
    delete: (id) => api.delete(`/api/roles/${id}`)
}

// Permissions endpoints
export const permissionsApi = {
    getAll: () => api.get('/api/permissions'),
    sync: () => api.post('/api/permissions/sync')
}

// Role Permissions endpoints
export const rolePermissionsApi = {
    get: (roleId) => api.get(`/api/roles/${roleId}/permissions`),
    update: (roleId, permissionIds) => api.put(`/api/roles/${roleId}/permissions`, { permissionIds })
}

// Medical Notes endpoints
export const medicalNotesApi = {
    getByAppointment: (appointmentId) => api.get(`/api/medicalnotes/by-appointment/${appointmentId}`),
    create: (data) => api.post('/api/medicalnotes', data),
    update: (id, data) => api.put(`/api/medicalnotes/${id}`, data),
    delete: (id) => api.delete(`/api/medicalnotes/${id}`)
}

// Users endpoints
export const usersApi = {
    getAll: () => api.get('/api/users'),
    getById: (id) => api.get(`/api/users/${id}`),
    getRoles: () => api.get('/api/users/roles'),
    create: (data) => api.post('/api/users', data),
    update: (id, data) => api.put(`/api/users/${id}`, data),
    delete: (id) => api.delete(`/api/users/${id}`)
}

export default api
