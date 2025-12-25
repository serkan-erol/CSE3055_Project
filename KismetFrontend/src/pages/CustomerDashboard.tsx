import { useState, useEffect } from 'react'
import { useNavigate, Outlet, useLocation } from 'react-router-dom'
import { sessionApi } from '../services/api'

const CustomerDashboard = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const [userId, setUserId] = useState<number | null>(null)

  // Get userId from token on component mount
  useEffect(() => {
    const loadCurrentUser = async () => {
      try {
        const userInfo = await sessionApi.getCurrentUser()
        if (userInfo.userId) {
          setUserId(userInfo.userId)
          // Also store in sessionStorage for quick access
          sessionStorage.setItem('userId', userInfo.userId.toString())
          if (userInfo.sessionId) {
            sessionStorage.setItem('sessionId', userInfo.sessionId.toString())
          }
        }
      } catch (err) {
        // If we can't get user info, redirect to login
        console.error('Failed to get current user:', err)
        navigate('/customer/login')
      }
    }
    loadCurrentUser()
  }, [navigate])

  const getActivePage = () => {
    const path = location.pathname
    if (path.includes('purchase-fabric')) return 'purchase-fabric'
    if (path.includes('supply-fabric')) return 'supply-fabric'
    if (path.includes('orders')) return 'orders'
    if (path.includes('payment-methods')) return 'payment-methods'
    if (path.includes('bank-information')) return 'bank-information'
    if (path.includes('settings')) return 'settings'
    return ''
  }

  const activePage = getActivePage()

  const handleNavigation = (path: string) => {
    navigate(`/customer/dashboard/${path}`)
  }

  const handleLogout = async () => {
    setIsLoggingOut(true)
    try {
      // Get userId from state or sessionStorage (prefer state, fallback to sessionStorage)
      const currentUserId = userId ?? (sessionStorage.getItem('userId') ? parseInt(sessionStorage.getItem('userId')!) : null)
      if (currentUserId) {
        await sessionApi.logout(currentUserId)
      }
      // Clear sessionStorage
      sessionStorage.removeItem('userId')
      sessionStorage.removeItem('sessionId')
      // Navigate to login page
      navigate('/customer/login', {
        state: { message: 'You have been logged out successfully.' }
      })
    } catch (err: any) {
      console.error('Logout error:', err)
      // Even if logout fails, clear session storage and redirect
      sessionStorage.removeItem('userId')
      sessionStorage.removeItem('sessionId')
      navigate('/customer/login')
    } finally {
      setIsLoggingOut(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Top Navigation Bar */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <button
                onClick={() => navigate('/customer/dashboard')}
                className="text-2xl font-bold text-primary-600 hover:text-primary-700 transition-colors cursor-pointer"
              >
                Kısmet
              </button>
            </div>
            <div className="flex items-center space-x-4">
              <button
                onClick={() => {
                  navigate('/customer/dashboard/settings')
                }}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
              >
                Settings
              </button>
              <button
                onClick={handleLogout}
                disabled={isLoggingOut}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {isLoggingOut ? 'Logging out...' : 'Logout'}
              </button>
            </div>
          </div>
        </div>
      </header>

      <div className="flex">
        {/* Sidebar */}
        <aside className="w-64 bg-white shadow-sm min-h-[calc(100vh-4rem)]">
          <nav className="p-4 space-y-2">
            <button
              onClick={() => handleNavigation('purchase-fabric')}
              className={`w-full text-left px-4 py-3 text-sm font-medium rounded-md transition-colors ${
                activePage === 'purchase-fabric'
                  ? 'bg-primary-100 text-primary-700 border-l-4 border-primary-600'
                  : 'text-gray-700 hover:bg-gray-100'
              }`}
            >
              Purchase Fabric
            </button>
            <button
              onClick={() => handleNavigation('supply-fabric')}
              className={`w-full text-left px-4 py-3 text-sm font-medium rounded-md transition-colors ${
                activePage === 'supply-fabric'
                  ? 'bg-primary-100 text-primary-700 border-l-4 border-primary-600'
                  : 'text-gray-700 hover:bg-gray-100'
              }`}
            >
              Supply Fabric
            </button>
            <button
              onClick={() => handleNavigation('orders')}
              className={`w-full text-left px-4 py-3 text-sm font-medium rounded-md transition-colors ${
                activePage === 'orders'
                  ? 'bg-primary-100 text-primary-700 border-l-4 border-primary-600'
                  : 'text-gray-700 hover:bg-gray-100'
              }`}
            >
              My Orders
            </button>
            <button
              onClick={() => handleNavigation('payment-methods')}
              className={`w-full text-left px-4 py-3 text-sm font-medium rounded-md transition-colors ${
                activePage === 'payment-methods'
                  ? 'bg-primary-100 text-primary-700 border-l-4 border-primary-600'
                  : 'text-gray-700 hover:bg-gray-100'
              }`}
            >
              Payment Methods
            </button>
            <button
              onClick={() => handleNavigation('bank-information')}
              className={`w-full text-left px-4 py-3 text-sm font-medium rounded-md transition-colors ${
                activePage === 'bank-information'
                  ? 'bg-primary-100 text-primary-700 border-l-4 border-primary-600'
                  : 'text-gray-700 hover:bg-gray-100'
              }`}
            >
              Bank Information
            </button>
          </nav>
        </aside>

        {/* Main Content Area */}
        <main className="flex-1 p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

export default CustomerDashboard

