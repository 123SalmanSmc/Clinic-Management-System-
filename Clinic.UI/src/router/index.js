import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue')
    },
    {
      path: '/',
      name: 'dashboard',
      component: () => import('../views/DashboardView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/patients',
      name: 'patients',
      component: () => import('../views/PatientsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/staff',
      name: 'staff',
      component: () => import('../views/StaffView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/appointments',
      name: 'appointments',
      component: () => import('../views/AppointmentsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/services',
      name: 'services',
      component: () => import('../views/ServicesView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/settings',
      name: 'settings',
      component: () => import('../views/SettingsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/staff-types',
      name: 'staff-types',
      component: () => import('../views/StaffTypesView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/tax',
      name: 'tax',
      component: () => import('../views/TaxView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/users',
      name: 'users',
      component: () => import('../views/UsersView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/specializations',
      name: 'specializations',
      component: () => import('../views/SpecializationsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/service-types',
      name: 'service-types',
      component: () => import('../views/ServiceTypesView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/payments',
      name: 'payments',
      component: () => import('../views/PaymentsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/roles',
      name: 'roles',
      component: () => import('../views/RolesView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/roles/:id/permissions',
      name: 'role-permissions',
      component: () => import('../views/RolePermissionsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/403',
      name: 'forbidden',
      component: () => import('../views/ForbiddenView.vue')
    }
  ]
})

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth) {
    if (!authStore.isAuthenticated) {
      // Try to check auth status with server
      const isAuth = await authStore.checkAuth()
      if (!isAuth) {
        next('/login')
        return
      }
    }

    // Check Permissions
    // If path is root '/' or '/dashboard', allow if authenticated (usually)
    // Or check hasPermission('/')
    if (to.path !== '/' && !authStore.hasPermission(to.path) && to.name !== 'forbidden') {
      // Check if it's a dynamic route like /roles/1/permissions. 
      // We might need to check the base path or exact match?
      // Ideally the hasPermission logic matches allowing patterns.
      // For now, let's assume route path matching.

      // Handle dynamic routes:
      let checkPath = to.path
      if (to.name === 'role-permissions') checkPath = '/roles' // Allow if access to roles? Or specific permission?
      // Actually, we should check exact permission. 
      // If user has '/roles', they can access '/roles'. 
      // For '/roles/:id/permissions', maybe we need a specific permission '/role-permissions' or just '/roles'.

      if (!authStore.hasPermission(checkPath)) {
        // next('/403') // Redirect to forbidden
        // For now, just warn or allow if Admin
        if (!authStore.isAdmin) {
          // next('/403')
          // Commenting out until ForbiddenView exists, redirect to dashboard
          next('/')
          return
        }
      }
    }
  }
  next()
})

export default router
