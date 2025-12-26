import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { employeeApi, sessionApi } from '../../services/api'

interface Employee {
  employeeID: number
  employeeNumber: string
  employeeRole: string
  accessLevel: number
  userName: string
  contactEmail: string
  contactPhone: string | null
  createdAt: string
  lastUpdatedAt: string | null
}

const ManageEmployee = () => {
  const navigate = useNavigate()
  const [employeeId, setEmployeeId] = useState<number | null>(null)
  const [currentAccessLevel, setCurrentAccessLevel] = useState<number | null>(null)
  const [employees, setEmployees] = useState<Employee[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  
  // Create employee form state
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [isCreating, setIsCreating] = useState(false)
  const [createError, setCreateError] = useState('')
  const [createSuccess, setCreateSuccess] = useState('')
  const [createForm, setCreateForm] = useState({
    userName: '',
    contactEmail: '',
    contactPhone: '',
    password: '',
    employeeRole: '',
    accessLevel: '3'
  })

  // Update state
  const [updatingEmployeeId, setUpdatingEmployeeId] = useState<number | null>(null)
  const [updateType, setUpdateType] = useState<'role' | 'accessLevel' | null>(null)
  const [newRole, setNewRole] = useState('')
  const [newAccessLevel, setNewAccessLevel] = useState('')
  const [isUpdating, setIsUpdating] = useState(false)
  const [updateError, setUpdateError] = useState('')
  const [updateSuccess, setUpdateSuccess] = useState('')

  useEffect(() => {
    const loadEmployeeInfo = async () => {
      try {
        const storedEmployeeId = sessionStorage.getItem('userId')
        const storedAccessLevel = sessionStorage.getItem('accessLevel')
        
        if (storedEmployeeId) {
          setEmployeeId(parseInt(storedEmployeeId))
        }
        if (storedAccessLevel) {
          const level = parseInt(storedAccessLevel)
          setCurrentAccessLevel(level)
          
          if (level < 5) {
            navigate('/employee/dashboard', {
              state: { message: 'You do not have permission to access this page. Access Level 5 or higher is required.' }
            })
            return
          }
        }
        
        const userInfo = await sessionApi.getCurrentUser()
        if (userInfo.userId) {
          setEmployeeId(userInfo.userId)
          if (storedEmployeeId !== userInfo.userId.toString()) {
            sessionStorage.setItem('userId', userInfo.userId.toString())
          }
          
          if (userInfo.accessLevel !== undefined) {
            setCurrentAccessLevel(userInfo.accessLevel)
            if (userInfo.accessLevel < 5) {
              navigate('/employee/dashboard', {
                state: { message: 'You do not have permission to access this page. Access Level 5 or higher is required.' }
              })
              return
            }
            if (storedAccessLevel !== userInfo.accessLevel.toString()) {
              sessionStorage.setItem('accessLevel', userInfo.accessLevel.toString())
            }
          }
        }
      } catch (err) {
        console.error('Failed to get employee info:', err)
        navigate('/employee/dashboard')
      }
    }
    loadEmployeeInfo()
  }, [navigate])

  useEffect(() => {
    if (employeeId && currentAccessLevel !== null && currentAccessLevel >= 5) {
      loadEmployees()
    }
  }, [employeeId, currentAccessLevel])

  const loadEmployees = async () => {
    if (!employeeId) return
    
    setIsLoading(true)
    setError('')
    try {
      const employeesData = await employeeApi.getAll(employeeId)
      setEmployees(Array.isArray(employeesData) ? employeesData : [])
    } catch (err: any) {
      console.error('Error loading employees:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to load employees'
      setError(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreateEmployee = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!employeeId) {
      setCreateError('Unable to create employee. Please refresh the page.')
      return
    }

    setIsCreating(true)
    setCreateError('')
    setCreateSuccess('')

    try {
      const employeeData = {
        userName: createForm.userName.trim(),
        contactEmail: createForm.contactEmail.trim(),
        contactPhone: createForm.contactPhone.trim() || undefined,
        password: createForm.password,
        employeeRole: createForm.employeeRole.trim(),
        accessLevel: parseInt(createForm.accessLevel)
      }

      // Validate
      if (!employeeData.userName || !employeeData.contactEmail || !employeeData.password || !employeeData.employeeRole) {
        setCreateError('All required fields must be filled')
        setIsCreating(false)
        return
      }

      if (isNaN(employeeData.accessLevel) || employeeData.accessLevel < 1 || employeeData.accessLevel > 10) {
        setCreateError('Access Level must be between 1 and 10')
        setIsCreating(false)
        return
      }

      await employeeApi.create(employeeId, employeeData)
      
      // Reset form
      setCreateForm({
        userName: '',
        contactEmail: '',
        contactPhone: '',
        password: '',
        employeeRole: '',
        accessLevel: '3'
      })
      setShowCreateForm(false)
      setCreateSuccess('Employee created successfully!')
      setTimeout(() => setCreateSuccess(''), 5000)
      
      // Reload employees
      await loadEmployees()
    } catch (err: any) {
      console.error('Error creating employee:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to create employee'
      setCreateError(errorMessage)
    } finally {
      setIsCreating(false)
    }
  }

  const handleUpdateRole = async (targetEmployeeId: number) => {
    if (!employeeId) {
      setUpdateError('Unable to update. Please refresh the page.')
      return
    }

    if (!newRole.trim()) {
      setUpdateError('Role cannot be empty')
      return
    }

    setIsUpdating(true)
    setUpdatingEmployeeId(targetEmployeeId)
    setUpdateError('')
    setUpdateSuccess('')

    try {
      await employeeApi.updateRole(employeeId, targetEmployeeId, newRole.trim())
      setUpdateType(null)
      setNewRole('')
      setUpdateSuccess('Role updated successfully!')
      setTimeout(() => setUpdateSuccess(''), 5000)
      await loadEmployees()
    } catch (err: any) {
      console.error('Error updating role:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to update role'
      setUpdateError(errorMessage)
    } finally {
      setIsUpdating(false)
      setUpdatingEmployeeId(null)
    }
  }

  const handleUpdateAccessLevel = async (targetEmployeeId: number) => {
    if (!employeeId) {
      setUpdateError('Unable to update. Please refresh the page.')
      return
    }

    const parsedLevel = parseInt(newAccessLevel)
    if (isNaN(parsedLevel) || parsedLevel < 1 || parsedLevel > 10) {
      setUpdateError('Access Level must be between 1 and 10')
      return
    }

    setIsUpdating(true)
    setUpdatingEmployeeId(targetEmployeeId)
    setUpdateError('')
    setUpdateSuccess('')

    try {
      await employeeApi.updateAccessLevel(employeeId, targetEmployeeId, parsedLevel)
      setUpdateType(null)
      setNewAccessLevel('')
      setUpdateSuccess('Access Level updated successfully!')
      setTimeout(() => setUpdateSuccess(''), 5000)
      await loadEmployees()
    } catch (err: any) {
      console.error('Error updating access level:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to update access level'
      setUpdateError(errorMessage)
    } finally {
      setIsUpdating(false)
      setUpdatingEmployeeId(null)
    }
  }

  const formatDate = (dateString: string | null) => {
    if (!dateString) return 'N/A'
    return new Date(dateString).toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  if (currentAccessLevel === null || currentAccessLevel < 5) {
    return (
      <div className="max-w-4xl mx-auto">
        <div className="bg-yellow-50 border border-yellow-200 rounded-md p-4">
          <div className="text-sm text-yellow-800">Loading access level...</div>
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Manage Employees</h2>
        <button
          onClick={() => {
            setShowCreateForm(!showCreateForm)
            setCreateError('')
            setCreateSuccess('')
          }}
          className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
        >
          {showCreateForm ? 'Cancel' : 'Create New Employee'}
        </button>
      </div>

      {/* Success Messages */}
      {createSuccess && (
        <div className="mb-4 bg-green-50 border border-green-200 rounded-md p-4">
          <div className="text-sm font-medium text-green-800">{createSuccess}</div>
        </div>
      )}

      {updateSuccess && (
        <div className="mb-4 bg-green-50 border border-green-200 rounded-md p-4">
          <div className="text-sm font-medium text-green-800">{updateSuccess}</div>
        </div>
      )}

      {/* Error Messages */}
      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
          <div className="text-sm font-medium text-red-800">Error: {error}</div>
        </div>
      )}

      {createError && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
          <div className="text-sm font-medium text-red-800">Error: {createError}</div>
        </div>
      )}

      {updateError && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
          <div className="text-sm font-medium text-red-800">Error: {updateError}</div>
        </div>
      )}

      {/* Create Employee Form */}
      {showCreateForm && (
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <h3 className="text-lg font-semibold text-gray-800 mb-4">Create New Employee</h3>

          <form onSubmit={handleCreateEmployee} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label htmlFor="userName" className="block text-sm font-medium text-gray-700 mb-1">
                  User Name <span className="text-red-500">*</span>
                </label>
                <input
                  id="userName"
                  type="text"
                  value={createForm.userName}
                  onChange={(e) => setCreateForm({ ...createForm, userName: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              <div>
                <label htmlFor="contactEmail" className="block text-sm font-medium text-gray-700 mb-1">
                  Email <span className="text-red-500">*</span>
                </label>
                <input
                  id="contactEmail"
                  type="email"
                  value={createForm.contactEmail}
                  onChange={(e) => setCreateForm({ ...createForm, contactEmail: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              <div>
                <label htmlFor="contactPhone" className="block text-sm font-medium text-gray-700 mb-1">
                  Phone
                </label>
                <input
                  id="contactPhone"
                  type="text"
                  value={createForm.contactPhone}
                  onChange={(e) => setCreateForm({ ...createForm, contactPhone: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              <div>
                <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
                  Password <span className="text-red-500">*</span>
                </label>
                <input
                  id="password"
                  type="password"
                  value={createForm.password}
                  onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              <div>
                <label htmlFor="employeeRole" className="block text-sm font-medium text-gray-700 mb-1">
                  Employee Role <span className="text-red-500">*</span>
                </label>
                <input
                  id="employeeRole"
                  type="text"
                  value={createForm.employeeRole}
                  onChange={(e) => setCreateForm({ ...createForm, employeeRole: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              <div>
                <label htmlFor="accessLevel" className="block text-sm font-medium text-gray-700 mb-1">
                  Access Level <span className="text-red-500">*</span>
                </label>
                <input
                  id="accessLevel"
                  type="number"
                  min="1"
                  max="10"
                  value={createForm.accessLevel}
                  onChange={(e) => setCreateForm({ ...createForm, accessLevel: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>
            </div>

            <div className="flex justify-end gap-4">
              <button
                type="button"
                onClick={() => {
                  setShowCreateForm(false)
                  setCreateError('')
                }}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isCreating}
                className="px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isCreating ? 'Creating...' : 'Create Employee'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Employees List */}
      <div className="bg-white rounded-lg shadow-md overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-800">All Employees</h3>
        </div>

        {isLoading ? (
          <div className="p-8 text-center">
            <div className="text-gray-500">Loading employees...</div>
          </div>
        ) : employees.length === 0 ? (
          <div className="p-8 text-center">
            <div className="text-gray-500">No employees found</div>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Role</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Access Level</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {employees.map((emp) => (
                  <tr key={emp.employeeID}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{emp.employeeID}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{emp.userName}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{emp.contactEmail}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {updateType === 'role' && updatingEmployeeId === emp.employeeID ? (
                        <div className="flex items-center gap-2">
                          <input
                            type="text"
                            value={newRole}
                            onChange={(e) => setNewRole(e.target.value)}
                            className="w-32 px-2 py-1 text-sm border border-gray-300 rounded-md"
                            disabled={isUpdating}
                          />
                          <button
                            onClick={() => handleUpdateRole(emp.employeeID)}
                            disabled={isUpdating}
                            className="px-2 py-1 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
                          >
                            Save
                          </button>
                          <button
                            onClick={() => {
                              setUpdateType(null)
                              setNewRole('')
                              setUpdateError('')
                            }}
                            className="px-2 py-1 text-xs bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
                          >
                            Cancel
                          </button>
                        </div>
                      ) : (
                        <div className="flex items-center gap-2">
                          <span>{emp.employeeRole}</span>
                          <button
                            onClick={() => {
                              setUpdateType('role')
                              setUpdatingEmployeeId(emp.employeeID)
                              setNewRole(emp.employeeRole)
                              setUpdateError('')
                            }}
                            className="text-xs text-blue-600 hover:text-blue-800"
                          >
                            Edit
                          </button>
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {updateType === 'accessLevel' && updatingEmployeeId === emp.employeeID ? (
                        <div className="flex items-center gap-2">
                          <input
                            type="number"
                            min="1"
                            max="10"
                            value={newAccessLevel}
                            onChange={(e) => setNewAccessLevel(e.target.value)}
                            className="w-20 px-2 py-1 text-sm border border-gray-300 rounded-md"
                            disabled={isUpdating}
                          />
                          <button
                            onClick={() => handleUpdateAccessLevel(emp.employeeID)}
                            disabled={isUpdating}
                            className="px-2 py-1 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
                          >
                            Save
                          </button>
                          <button
                            onClick={() => {
                              setUpdateType(null)
                              setNewAccessLevel('')
                              setUpdateError('')
                            }}
                            className="px-2 py-1 text-xs bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
                          >
                            Cancel
                          </button>
                        </div>
                      ) : (
                        <div className="flex items-center gap-2">
                          <span>{emp.accessLevel}</span>
                          <button
                            onClick={() => {
                              setUpdateType('accessLevel')
                              setUpdatingEmployeeId(emp.employeeID)
                              setNewAccessLevel(emp.accessLevel.toString())
                              setUpdateError('')
                            }}
                            className="text-xs text-blue-600 hover:text-blue-800"
                          >
                            Edit
                          </button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}

export default ManageEmployee

