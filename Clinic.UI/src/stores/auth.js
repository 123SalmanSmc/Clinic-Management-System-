import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { authApi } from '../services/api'

export const useAuthStore = defineStore('auth', () => {
    // With HTTP-only cookies, we don't store the token in JS
    // We only track if the user is authenticated based on /me endpoint
    const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))
    const permissions = ref(JSON.parse(localStorage.getItem('permissions') || '[]'))
    const isLoading = ref(false)

    const isAuthenticated = computed(() => !!user.value)
    const isAdmin = computed(() => user.value?.role === 'Admin' || user.value?.roleId === 1)

    function hasPermission(routePath) {
        if (isAdmin.value) return true
        if (!permissions.value || permissions.value.length === 0) return false
        // Check if routePath matches any permission path
        // For simplicity, exact match or startsWith
        return permissions.value.some(p => p === routePath || routePath.startsWith(p))
    }

    async function login(username, password) {
        try {
            isLoading.value = true
            await authApi.login(username, password)

            // After successful login, fetch user info
            const meResponse = await authApi.me()
            user.value = {
                username: meResponse.data.username,
                role: meResponse.data.role,
                roleId: meResponse.data.roleId,
                userId: meResponse.data.userId,
                staffId: meResponse.data.staffId
            }
            permissions.value = meResponse.data.permissions || []

            localStorage.setItem('user', JSON.stringify(user.value))
            localStorage.setItem('permissions', JSON.stringify(permissions.value))

            return true
        } catch (error) {
            console.error('Login failed:', error)
            return false
        } finally {
            isLoading.value = false
        }
    }

    async function register(username, password, roleId) {
        try {
            const response = await authApi.register(username, password, roleId)
            return response.data
        } catch (error) {
            console.error('Registration failed:', error)
            throw error
        }
    }

    async function logout() {
        try {
            await authApi.logout()
        } catch (error) {
            console.error('Logout error:', error)
        } finally {
            user.value = null
            permissions.value = []
            localStorage.removeItem('user')
            localStorage.removeItem('permissions')
        }
    }

    async function checkAuth() {
        try {
            const response = await authApi.me()
            user.value = {
                username: response.data.username,
                role: response.data.role,
                roleId: response.data.roleId,
                userId: response.data.userId,
                staffId: response.data.staffId
            }
            permissions.value = response.data.permissions || []
            localStorage.setItem('user', JSON.stringify(user.value))
            localStorage.setItem('permissions', JSON.stringify(permissions.value))
            return true
        } catch (error) {
            user.value = null
            permissions.value = []
            localStorage.removeItem('user')
            localStorage.removeItem('permissions')
            return false
        }
    }

    return {
        user,
        permissions,
        isLoading,
        isAuthenticated,
        isAdmin,
        hasPermission,
        login,
        register,
        logout,
        checkAuth
    }
})
