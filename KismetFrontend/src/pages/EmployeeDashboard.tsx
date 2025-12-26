import { useState, useEffect } from 'react'
import { useNavigate, Outlet, useLocation } from 'react-router-dom'
import { sessionApi } from '../services/api'

const EmployeeDashboard = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const [userId, setUserId] = useState<number | null>(null)
  //aaa Will be implemented later to decide/limit an employees acces to certain api-endpoints
  const [accessLevel, setAccessLevel] = useState<number | null>(null)

  // Get userId from token on component mount and verify user type
  useEffect(() => {
    const loadCurrentUser = async () => {
      try {
        const userInfo = await sessionApi.getCurrentUser()
        if (userInfo.userId) {
          // Check user type - redirect customers to customer dashboard
          if (userInfo.userType === 'Customer') {
            // Customer trying to access employee dashboard - redirect to customer dashboard
            navigate('/customer/dashboard')
            return
          } else if (userInfo.userType === 'Employee') {
            setUserId(userInfo.userId)
            // Also store in sessionStorage for quick access
            sessionStorage.setItem('userId', userInfo.userId.toString())
            if (userInfo.sessionId) {
              sessionStorage.setItem('sessionId', userInfo.sessionId.toString())
            }
            // Store access level if available
            if (userInfo.accessLevel !== undefined) {
              setAccessLevel(userInfo.accessLevel)
            }
            if (userInfo.role) {
              sessionStorage.setItem('role', userInfo.role)
            }
            if (userInfo.accessLevel) {
              sessionStorage.setItem('accessLevel', userInfo.accessLevel.toString())
            }
            // TODO: Fetch access level from employee endpoint when available
            // For now, we'll show all buttons. Access level filtering will be added later.
          } else {
            // Invalid user type
            navigate('/employee/login')
          }
        }
      } catch (err) {
        // If we can't get user info, redirect to login
        console.error('Failed to get current user:', err)
        navigate('/employee/login')
      }
    }
    loadCurrentUser()
  }, [navigate])

  const getActivePage = () => {
    const path = location.pathname
    if (path.includes('check-order')) return 'check-order'
    if (path.includes('check-shipment')) return 'check-shipment'
    if (path.includes('manage-fabric')) return 'manage-fabric'
    if (path.includes('manage-finance')) return 'manage-finance'
    if (path.includes('manage-payment')) return 'manage-payment'
    if (path.includes('manage-treasury')) return 'manage-treasury'
    if (path.includes('manage-employee')) return 'manage-employee'
    if (path.includes('views')) return 'views'
    if (path.includes('settings')) return 'settings'
    return ''
  }

  const activePage = getActivePage()

  const handleNavigation = (path: string) => {
    navigate(`/employee/dashboard/${path}`)
  }

  const handleLogout = async () => {
    setIsLoggingOut(true)
    try {
      // Get userId from state or sessionStorage (prefer state, fallback to sessionStorage)
      const currentUserId = userId ?? (sessionStorage.getItem('userId') ? parseInt(sessionStorage.getItem('userId')!) : null)
      if (currentUserId) {
        await sessionApi.logout(currentUserId)
      }
      // Clear all sessionStorage
      sessionStorage.clear()
      // Clear all cookies
      document.cookie.split(";").forEach((c) => {
        document.cookie = c
          .replace(/^ +/, "")
          .replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/")
      })
      // Navigate to login page
      navigate('/employee/login', {
        state: { message: 'You have been logged out successfully.' }
      })
    } catch (err: any) {
      console.error('Logout error:', err)
      // Even if logout fails, clear session storage and cookies and redirect
      sessionStorage.clear()
      document.cookie.split(";").forEach((c) => {
        document.cookie = c
          .replace(/^ +/, "")
          .replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/")
      })

      navigate('/employee/login')
    } finally {
      setIsLoggingOut(false)
    }
  }

  // Function to determine which sidebar buttons to show based on access level
  const getSidebarButtons = () => {
    const buttons = []

    // Check Order - available for all employees (for now)
    if (accessLevel !== null && accessLevel >= 3) {
      buttons.push({
        key: 'check-order',
        label: 'Check Order',
        path: 'check-order',
        accessLevel: 3
      })
    }

    // Check Shipment - requires AccessLevel >= 3
    if (accessLevel !== null && accessLevel >= 3) {
      buttons.push({
        key: 'check-shipment',
        label: 'Check Shipment',
        path: 'check-shipment',
        accessLevel: 3
      })
    }

    // Manage Fabric - requires AccessLevel >= 5
    if (accessLevel !== null && accessLevel >= 5) {
      buttons.push({
        key: 'manage-fabric',
        label: 'Manage Fabric',
        path: 'manage-fabric',
        accessLevel: 5
      })
    }

    // Manage Employee - requires AccessLevel >= 5
    if (accessLevel !== null && accessLevel >= 5) {
      buttons.push({
        key: 'manage-employee',
        label: 'Manage Employee',
        path: 'manage-employee',
        accessLevel: 5
      })
    }

    // Finance, Payment, Treasury, Views - require AccessLevel >= 4
    if (accessLevel !== null && accessLevel >= 4) {
      buttons.push({
        key: 'manage-finance',
        label: 'Finance',
        path: 'manage-finance',
        accessLevel: 4
      })
      buttons.push({
        key: 'manage-payment',
        label: 'Payment',
        path: 'manage-payment',
        accessLevel: 4
      })
      buttons.push({
        key: 'manage-treasury',
        label: 'Treasury',
        path: 'manage-treasury',
        accessLevel: 4
      })
      buttons.push({
        key: 'views',
        label: 'Views',
        path: 'views',
        accessLevel: 4
      })
    }

    // Add more buttons here based on access level
    // Example:
    // if (accessLevel !== null && accessLevel >= 5) {
    //   buttons.push({ key: 'manage-orders', label: 'Manage Orders', path: 'manage-orders' })
    // }

    return buttons
  }

  const sidebarButtons = getSidebarButtons()

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Top Navigation Bar */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <button
                onClick={() => navigate('/employee/dashboard')}
                className="text-2xl font-bold text-primary-600 hover:text-primary-700 transition-colors cursor-pointer"
              >
                Kısmet
              </button>
            </div>
            <div className="flex items-center space-x-4">
              <button
                onClick={() => {
                  navigate('/employee/dashboard/settings')
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
            {sidebarButtons.map((button) => (
              <button
                key={button.key}
                onClick={() => handleNavigation(button.path)}
                className={`w-full text-left px-4 py-3 text-sm font-medium rounded-md transition-colors ${
                  activePage === button.key
                    ? 'bg-primary-100 text-primary-700 border-l-4 border-primary-600'
                    : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                {button.label}
              </button>
            ))}
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

export default EmployeeDashboard

