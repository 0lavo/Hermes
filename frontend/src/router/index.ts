import { createRouter, createWebHistory } from 'vue-router'
import HomeHermes from '../views/HomeHermes.vue'
import HomeView from '../views/HomeView.vue'
import DesignSystemTest from '../views/DesignSystemTest.vue'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import MyRooms from '../views/MyRooms.vue'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeHermes,
    },
    {
      path: '/HomeHermes',
      redirect: '/',
    },
    {
      path: '/app',
      name: 'app-home',
      component: HomeView,
      meta: { requiresAuth: true },
    },
    {
      path: '/test',
      name: 'design-test',
      component: DesignSystemTest,
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
    },
    {
      path: '/MyRooms',
      name: 'MyRooms',
      component: MyRooms,
      meta: { requiresAuth: true },
    },
  ],
})

router.beforeEach(async (to) => {
  const authStore = useAuthStore()

  // Recuperar a sessão caso exista um token guardado
  if (authStore.token && !authStore.user) {
    await authStore.restoreSession()
  }

  // Proteger páginas privadas
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return '/login'
  }

  // Impedir o acesso ao Login e Registo com sessão iniciada
  if ((to.path === '/login' || to.path === '/register') && authStore.isAuthenticated) {
    return '/MyRooms'
  }
})

export default router