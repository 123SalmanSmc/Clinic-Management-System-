<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import MainLayout from '../layouts/MainLayout.vue'
import { rolesApi, permissionsApi, rolePermissionsApi } from '../services/api'

const route = useRoute()
const router = useRouter()
const roleId = route.params.id
const role = ref(null)
const allPermissions = ref([])
const rolePermissions = ref([]) // IDs
const loading = ref(true)

// Group permissions by 'Group' property
const groupedPermissions = computed(() => {
    const groups = {}
    allPermissions.value.forEach(p => {
        const g = p.group || 'Other'
        if (!groups[g]) groups[g] = []
        groups[g].push(p)
    })
    return groups
})

onMounted(async () => {
    try {
        const [roleRes, allPermRes, rolePermRes] = await Promise.all([
            rolesApi.get(roleId),
            permissionsApi.getAll(),
            rolePermissionsApi.get(roleId)
        ])
        role.value = roleRes.data
        allPermissions.value = allPermRes.data
        rolePermissions.value = rolePermRes.data.map(rp => rp.permissionId)
    } catch (e) {
        console.error(e)
        alert('Failed to load data')
    } finally {
        loading.value = false
    }
})

const togglePermission = (permId) => {
    if (rolePermissions.value.includes(permId)) {
        rolePermissions.value = rolePermissions.value.filter(id => id !== permId)
    } else {
        rolePermissions.value.push(permId)
    }
}

const save = async () => {
    try {
        await rolePermissionsApi.update(roleId, rolePermissions.value)
        alert('Permissions updated successfully!')
        router.push('/roles')
    } catch (e) {
        alert('Failed to update permissions')
    }
}

const sync = async () => {
    await permissionsApi.sync()
    const res = await permissionsApi.getAll()
    allPermissions.value = res.data
}
</script>

<template>
  <MainLayout>
    <div class="header">
        <h1>Permissions for {{ role?.name }}</h1>
        <div class="actions">
            <button class="btn" @click="sync">🔄 Sync Routes</button>
            <button class="btn btn-primary" @click="save">Save Changes</button>
        </div>
    </div>

    <div v-if="loading" class="loading">Loading...</div>
    
    <div v-else class="groups-grid">
        <div v-for="(perms, groupName) in groupedPermissions" :key="groupName" class="group-card">
            <h3>{{ groupName }}</h3>
            <div class="perm-list">
                <label v-for="perm in perms" :key="perm.id" class="perm-item">
                    <input type="checkbox" :checked="rolePermissions.includes(perm.id)" @change="togglePermission(perm.id)" />
                    <span class="perm-name">{{ perm.name }}</span>
                    <span class="perm-path">{{ perm.routePath }}</span>
                </label>
            </div>
        </div>
    </div>
  </MainLayout>
</template>

<style scoped>
.header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.actions { display: flex; gap: 10px; }
.groups-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; }
.group-card { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
.group-card h3 { margin-bottom: 15px; border-bottom: 1px solid #eee; padding-bottom: 10px; color: var(--color-primary); }
.perm-list { display: flex; flex-direction: column; gap: 10px; }
.perm-item { display: flex; align-items: center; gap: 10px; cursor: pointer; padding: 5px; border-radius: 4px; }
.perm-item:hover { background: #f9f9f9; }
.perm-name { font-weight: 500; }
.perm-path { font-size: 0.8em; color: gray; margin-left: auto; }
</style>
