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
  cancelOrder: async (employeeId: number, orderId: number) => {
    const response = await api.put(`/Order/${employeeId}/${orderId}/cancel-order/`)
    return response.data
  },
  createOrder: async (customerId: number, data: {
    orderType: string
    fabricIDs: number[]
    quantities: number[]
    qualityGrades?: (string | null)[]
  }) => {
    const response = await api.post(`/Order/${customerId}/create-order`, data)
    return response.data
  },
}

// Fabric API functions
export const fabricApi = {
  getAll: async () => {
    const response = await api.get('/Fabric')
    return response.data
  },
  getById: async (fabricId: number) => {
    const response = await api.get(`/Fabric/${fabricId}`)
    return response.data
  },
  create: async (employeeId: number, data: {
    fabricType: string
    composition?: string
    color?: string
    weightPerUnit?: number
    stockQuantity: number
    unitPrice: number
    description?: string
  }) => {
    const response = await api.post(`/Fabric/${employeeId}/create-fabric`, data)
    return response.data
  },
  updateStock: async (fabricId: number, quantityChange: number) => {
    const response = await api.patch(`/Fabric/${fabricId}/stock`, {
      quantityChange: quantityChange
    })
    return response.data
  },
}

// Payment Methods (Cards) API functions
export const paymentMethodApi = {
  getAllByCustomerId: async (customerId: number) => {
    const response = await api.get(`/CustomerPayment/customers/${customerId}/payment-methods`)
    return response.data
  },
  getById: async (spmId: number) => {
    const response = await api.get(`/CustomerPayment/payment-methods/${spmId}`)
    return response.data
  },
  create: async (data: {
    customerID: number
    cardNumber: string
    cardType: string
    cardExpirationDate: string
    recordExpirationDate?: string
  }) => {
    const response = await api.post('/CustomerPayment/payment-methods', data)
    return response.data
  },
  update: async (spmId: number, data: {
    cardNumber: string
    cardType: string
    cardExpirationDate: string
    recordExpirationDate?: string
  }) => {
    const response = await api.put(`/CustomerPayment/payment-methods/${spmId}`, data)
    return response.data
  },
  delete: async (spmId: number) => {
    const response = await api.delete(`/CustomerPayment/payment-methods/${spmId}`)
    return response.data
  },
}

// Bank Information API functions
export const bankInfoApi = {
  getAllByCustomerId: async (customerId: number) => {
    const response = await api.get(`/CustomerPayment/customers/${customerId}/bank-information`)
    return response.data
  },
  getById: async (sbiId: number) => {
    const response = await api.get(`/CustomerPayment/bank-information/${sbiId}`)
    return response.data
  },
  create: async (data: {
    customerID: number
    bankName?: string
    accountNo?: string
    iban?: string
  }) => {
    const response = await api.post('/CustomerPayment/bank-information', data)
    return response.data
  },
  update: async (sbiId: number, data: {
    bankName?: string
    accountNo?: string
    iban?: string
  }) => {
    const response = await api.put(`/CustomerPayment/bank-information/${sbiId}`, data)
    return response.data
  },
  delete: async (sbiId: number) => {
    const response = await api.delete(`/CustomerPayment/bank-information/${sbiId}`)
    return response.data
  },
}

// Shipment API functions
export const shipmentApi = {
  updateShipmentStatus: async (employeeId: number, shipmentId: number, shipmentStatus: number) => {
    const response = await api.patch(`/Shipment/${employeeId}/${shipmentId}/status`, {
      shipmentStatus: shipmentStatus
    })
    return response.data
  },
  updateShipmentCountries: async (employeeId: number, shipmentId: number, data: {
    orderType: string
    originCountry?: string
    destinationCountry?: string
  }) => {
    const response = await api.patch(`/Shipment/${employeeId}/${shipmentId}/countries`, data)
    return response.data
  },
  getShipmentsByOrderId: async (orderId: number) => {
    const response = await api.get(`/Shipment/order/${orderId}`)
    return response.data
  },
  getShipmentById: async (shipmentId: number) => {
    const response = await api.get(`/Shipment/${shipmentId}`)
    return response.data
  },
}

// Financial Transaction API functions
export const financialTransactionApi = {
  getByCustomerId: async (employeeId: number, customerId: number) => {
    const response = await api.get(`/FinancialTransaction/${employeeId}/${customerId}/get-all-customer-financial-transactions/for-employees`)
    return response.data
  },
  getById: async (employeeId: number, fTransactionId: number) => {
    const response = await api.get(`/FinancialTransaction/${employeeId}/${fTransactionId}/get-by-fTransaction-id/for-employees`)
    return response.data
  },
  updateDescription: async (employeeId: number, fTransactionId: number, description: string) => {
    const response = await api.put(`/FinancialTransaction/${employeeId}/${fTransactionId}/update-description/`, {
      description: description
    })
    return response.data
  },
}

// Payment API functions
export const paymentApi = {
  getById: async (paymentId: number) => {
    const response = await api.get(`/Payment/${paymentId}/get-by-payment-id/`)
    return response.data
  },
  getByCustomerId: async (customerId: number) => {
    const response = await api.get(`/Payment/${customerId}/get-all-customer-payments`)
    return response.data
  },
  create: async (customerId: number, fTransactionId: number, data: {
    paymentAmount: number
    paymentType: string
    paymentMethod?: string
    referenceNumber?: string
  }) => {
    const response = await api.post(`/Payment/create-payment/?customerId=${customerId}&fTransactionId=${fTransactionId}`, {
      paymentAmount: data.paymentAmount,
      paymentType: data.paymentType,
      paymentMethod: data.paymentMethod,
      referenceNumber: data.referenceNumber
    })
    return response.data
  },
}

// Financial Transaction API functions for customers
export const financialTransactionApiCustomer = {
  getAll: async (customerId: number) => {
    const response = await api.get(`/FinancialTransaction/${customerId}/get-all-customer-financial-transactions/for-customers`)
    return response.data
  },
}

// Views API functions
export const viewsApi = {
  getOrderDetails: async (customerId: number) => {
    const response = await api.get(`/Views/orders/customer/${customerId}`)
    return response.data
  },
  getShipmentTracking: async (customerId: number) => {
    const response = await api.get(`/Views/shipments/customer/${customerId}`)
    return response.data
  },
  getFinancialOverview: async (customerId: number) => {
    const response = await api.get(`/Views/financial/customer/${customerId}`)
    return response.data
  },
  getInventoryStatus: async (fabricId: number) => {
    const response = await api.get(`/Views/inventory/fabric/${fabricId}`)
    return response.data
  },
}

// Treasury API functions
export const treasuryApi = {
  getBalance: async (employeeId: number) => {
    const response = await api.get(`/Treasury/${employeeId}/balance`)
    return response.data
  },
  getAll: async (employeeId: number) => {
    const response = await api.get(`/Treasury/${employeeId}`)
    return response.data
  },
  getById: async (employeeId: number, treasuryId: number) => {
    const response = await api.get(`/Treasury/${employeeId}/${treasuryId}/get-by-id`)
    return response.data
  },
  getByTransactionId: async (employeeId: number, fTransactionId: number) => {
    const response = await api.get(`/Treasury/${employeeId}/${fTransactionId}/get-by-transaction-id`)
    return response.data
  },
}

// Employee API functions
export const employeeApi = {
  getAll: async (employeeId: number) => {
    const response = await api.get(`/Employee/${employeeId}/all-employees`)
    return response.data
  },
  getById: async (employeeId: number) => {
    const response = await api.get(`/Employee/${employeeId}/employee-by-id`)
    return response.data
  },
  create: async (employeeId: number, data: {
    userName: string
    contactEmail: string
    contactPhone?: string
    password: string
    employeeRole: string
    accessLevel: number
  }) => {
    const response = await api.post(`/Employee/${employeeId}/create-employee`, data)
    return response.data
  },
  updateRole: async (updaterEmployeeId: number, employeeId: number, employeeRole: string) => {
    const response = await api.put(`/Employee/${updaterEmployeeId}/role`, {
      employeeID: employeeId,
      employeeRole: employeeRole
    })
    return response.data
  },
  updateAccessLevel: async (updaterEmployeeId: number, employeeId: number, accessLevel: number) => {
    const response = await api.put(`/Employee/${updaterEmployeeId}/access-level`, {
      employeeID: employeeId,
      accessLevel: accessLevel
    })
    return response.data
  },
}

export default api

