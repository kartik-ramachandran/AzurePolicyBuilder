import { createRouter, createWebHistory } from 'vue-router'
import ProjectBuilder from '@/views/ProjectBuilder.vue'
import PolicyEditor from '@/views/PolicyEditor.vue'
import TemplateLibrary from '@/views/TemplateLibrary.vue'
import FragmentManager from '@/views/FragmentManager.vue'
import ApiBuilder from '@/views/ApiBuilder.vue'
import ApiDocumentation from '@/views/ApiDocumentation.vue'
import ApiTester from '@/views/ApiTester.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'project',
      component: ProjectBuilder
    },
    {
      path: '/editor',
      name: 'editor',
      component: PolicyEditor
    },
    {
      path: '/templates',
      name: 'templates',
      component: TemplateLibrary
    },
    {
      path: '/fragments',
      name: 'fragments',
      component: FragmentManager
    },
    {
      path: '/api-builder',
      name: 'api-builder',
      component: ApiBuilder
    },
    {
      path: '/api-docs',
      name: 'api-docs',
      component: ApiDocumentation
    },
    {
      path: '/api-tester',
      name: 'api-tester',
      component: ApiTester
    }
  ]
})

export default router
