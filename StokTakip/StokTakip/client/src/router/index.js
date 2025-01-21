import { createRouter, createWebHistory } from 'vue-router';
import LoginForm from '@/components/LoginForm.vue'; // Giriş ekranı
import IndexPage from '@/components/IndexPage.vue'; // Sol navbar sayfası

const routes = [
    {
        path: '/',
        name: 'Login',
        component: LoginForm,
    },
    {
        path: '/Dashboard',
        name: 'Dashboard',
        component: IndexPage,
    },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;
