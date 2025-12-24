import axios from 'axios'

const API_BASE_URL = 'http://localhost:3055/api'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Required to send cookies with requests
})

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid - redirect to appropriate login page
      // Cookies will be cleared by the backend on logout
      const currentPath = window.location.pathname
      
      // Don't redirect if we're already on a login page
      if (currentPath.includes('/employee/login') || currentPath.includes('/customer/login')) {
        return Promise.reject(error)
      }
      
      // Redirect to employee login if on employee dashboard, otherwise customer login
      if (currentPath.includes('/employee/')) {
        window.location.href = '/employee/login'
      } else {
        window.location.href = '/customer/login'
      }
    }
    return Promise.reject(error)
  }
)

// Customer API functions
export const customerApi = {
  register: async (data: {
    userName: string
    contactEmail: string
    contactPhone?: string
    password: string
    customerType?: string
    reliabilityStatus?: boolean
    city?: string
    country?: string
  }) => {
    const response = await api.post('/Customer', data)
    return response.data
  },
}

// Session API functions
export const sessionApi = {
  login: async (data: {
    contactEmail: string
    password: string
  }) => {
    const response = await api.post('/Session/login', data)
    return response.data
  },
  logout: async (userId: number) => {
    const response = await api.post(`/Session/logout/${userId}`)
    return response.data
  },
  getCurrentUser: async () => {
    const response = await api.get('/Session/current-user')
    return response.data
  },
}

// User API functions
export const userApi = {
  getById: async (userId: number) => {
    const response = await api.get(`/User/${userId}`)
    return response.data
  },
  updateName: async (userId: number, userName: string) => {
    const response = await api.put(`/User/${userId}/name`, { userName })
    return response.data
  },
  updateEmail: async (userId: number, contactEmail: string) => {
    const response = await api.put(`/User/${userId}/email`, { contactEmail })
    return response.data
  },
  updatePhone: async (userId: number, contactPhone: string) => {
    const response = await api.put(`/User/${userId}/phone`, { contactPhone })
    return response.data
  },
  updatePassword: async (userId: number, data: {
    contactEmail: string
    oldPassword: string
    newPassword: string
  }) => {
    const response = await api.put(`/User/${userId}/password`, data)
    return response.data
  },
}

// Order API functions
export const orderApi = {
  getAllForCustomer: async (customerId: number) => {
    const response = await api.get(`/Order/${customerId}/all-orders/for-customers`)
    return response.data
  },
  getByIdForEmployee: async (orderId: number) => {
    const response = await api.get(`/Order/${orderId}/order-by-id/for-employees`)
    return response.data
  },
  approveOrder: async (employeeId: number, orderId: number) => {
    const response = await api.put(`/Order/${employeeId}/approve-order/`, { orderID: orderId })
    return response.data
  },
}

export default api

