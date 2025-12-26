import { useState, useEffect } from 'react'
import { useNavigate, Link, useLocation } from 'react-router-dom'
import { Eye, EyeOff } from 'lucide-react'
import { sessionApi } from '../services/api'

const EmployeeLogin = () => {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [showPassword, setShowPassword] = useState(false)
  const navigate = useNavigate()
  const location = useLocation()

  // Check if user is already logged in
  useEffect(() => {
    const checkLoginStatus = async () => {
      try {
        const userInfo = await sessionApi.getCurrentUser()
        if (userInfo.userId && userInfo.userType === 'Employee') {
          navigate('/employee/dashboard')
        } else if (userInfo.userId && userInfo.userType === 'Customer') {
          // If customer is logged in, redirect to customer dashboard
          navigate('/customer/dashboard')
        }
      } catch (error) {
        // Not logged in, continue to login page
      }
    }
    checkLoginStatus()

    // Display message from location state (e.g., from logout)
    if (location.state?.message) {
      // You can show this as a success message if needed
    }
  }, [navigate, location])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      const loginData = {
        contactEmail: email,
        password: password,
      }

      const loginResponse = await sessionApi.login(loginData)
      
      // Store userId and sessionId in sessionStorage
      if (loginResponse.userId) {
        sessionStorage.setItem('userId', loginResponse.userId.toString())
      }
      if (loginResponse.sessionId) {
        sessionStorage.setItem('sessionId', loginResponse.sessionId.toString())
      }
      if (loginResponse.accessLevel) {
        sessionStorage.setItem('accessLevel', loginResponse.accessLevel.toString())
      }
      if (loginResponse.role) {
        sessionStorage.setItem('role', loginResponse.role)
      }

      // Verify user type before redirecting
      const userInfo = await sessionApi.getCurrentUser()
      if (userInfo.userType === 'Employee') {
        navigate('/employee/dashboard', {
          state: { message: 'Login successful! Welcome back.' }
        })
      } else if (userInfo.userType === 'Customer') {
        // Customer logged in through employee login - redirect to customer dashboard
        navigate('/customer/dashboard', {
          state: { message: 'Login successful! Welcome back.' }
        })
      } else {
        setError('Invalid user type. Please contact support.')
      }
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Login failed. Please check your credentials.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex flex-col bg-gradient-to-br from-gray-50 to-gray-100">
      {/* Header with Kısmet logo */}
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center h-16">
            <button
              onClick={() => navigate('/')}
              className="text-2xl font-bold text-primary-600 hover:text-primary-700 transition-colors cursor-pointer"
            >
              Kısmet
            </button>
          </div>
        </div>
      </header>

      <div className="flex-1 flex items-center justify-center">
        <div className="max-w-md w-full space-y-8 p-8 bg-white rounded-xl shadow-lg">
          <div>
            <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
              Employee Login
            </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Sign in to your employee account
          </p>
        </div>
        <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
          {error && (
            <div className="rounded-md bg-red-50 p-4">
              <div className="text-sm text-red-800">{error}</div>
            </div>
          )}
          <div className="rounded-md shadow-sm -space-y-px">
            <div>
              <label htmlFor="email" className="sr-only">
                Email address
              </label>
              <input
                id="email"
                name="email"
                type="email"
                autoComplete="email"
                required
                className="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-t-md focus:outline-none focus:ring-gray-500 focus:border-gray-500 focus:z-10 sm:text-sm"
                placeholder="Email address"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>
            <div>
              <label htmlFor="password" className="sr-only">
                Password
              </label>
              <div className="relative">
                <input
                  id="password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  required
                  className="appearance-none rounded-none relative block w-full px-3 py-2 pr-10 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-b-md focus:outline-none focus:ring-gray-500 focus:border-gray-500 focus:z-10 sm:text-sm"
                  placeholder="Password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-500 hover:text-gray-700"
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>
          </div>

          <div>
            <button
              type="submit"
              disabled={isLoading}
              className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-gray-700 hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Signing in...' : 'Sign in'}
            </button>
          </div>

          <div className="text-center">
            <Link
              to="/"
              className="text-sm text-gray-600 hover:text-gray-500"
            >
              Back to home
            </Link>
          </div>
        </form>
        </div>
      </div>
    </div>
  )
}

export default EmployeeLogin

