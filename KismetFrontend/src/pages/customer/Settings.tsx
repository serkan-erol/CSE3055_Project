import { useState, useEffect } from 'react'
import { Eye, EyeOff } from 'lucide-react'
import { userApi, sessionApi } from '../../services/api'

interface UserInfo {
  userID: number
  userName: string
  contactEmail: string
  contactPhone?: string
}

const Settings = () => {
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null)
  const [loading, setLoading] = useState(true)

  // Form states
  const [nameForm, setNameForm] = useState({ userName: '' })
  const [emailForm, setEmailForm] = useState({ contactEmail: '' })
  const [phoneForm, setPhoneForm] = useState({ contactPhone: '' })
  const [passwordForm, setPasswordForm] = useState({
    oldPassword: '',
    newPassword: '',
    confirmPassword: '',
  })

  // Loading states for each form
  const [updatingName, setUpdatingName] = useState(false)
  const [updatingEmail, setUpdatingEmail] = useState(false)
  const [updatingPhone, setUpdatingPhone] = useState(false)
  const [updatingPassword, setUpdatingPassword] = useState(false)

  // Password visibility states
  const [showOldPassword, setShowOldPassword] = useState(false)
  const [showNewPassword, setShowNewPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)

  // Individual error and success messages for each form
  const [nameError, setNameError] = useState('')
  const [nameSuccess, setNameSuccess] = useState('')
  const [emailError, setEmailError] = useState('')
  const [emailSuccess, setEmailSuccess] = useState('')
  const [phoneError, setPhoneError] = useState('')
  const [phoneSuccess, setPhoneSuccess] = useState('')
  const [passwordError, setPasswordError] = useState('')
  const [passwordSuccess, setPasswordSuccess] = useState('')

  useEffect(() => {
    loadUserInfo()
  }, [])

  const loadUserInfo = async () => {
    try {
      setLoading(true)
      const currentUser = await sessionApi.getCurrentUser()
      if (currentUser.userId) {
        const user = await userApi.getById(currentUser.userId)
        setUserInfo(user)
        setNameForm({ userName: user.userName || '' })
        setEmailForm({ contactEmail: user.contactEmail || '' })
        setPhoneForm({ contactPhone: user.contactPhone || '' })
      }
    } catch (err: any) {
      console.error('Failed to load user information:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleUpdateName = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!userInfo) return

    // Check if the new name is the same as the current name
    if (nameForm.userName.trim() === userInfo.userName.trim()) {
      setNameError('')
      setNameSuccess('The name is the same as the current one. No update needed.')
      return
    }

    setUpdatingName(true)
    setNameError('')
    setNameSuccess('')

    try {
      await userApi.updateName(userInfo.userID, nameForm.userName)
      setNameSuccess('Name updated successfully!')
      await loadUserInfo() // Reload to get updated info
    } catch (err: any) {
      setNameError(err.response?.data?.error || 'Failed to update name')
    } finally {
      setUpdatingName(false)
    }
  }

  const handleUpdateEmail = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!userInfo) return

    // Check if the new email is the same as the current email
    if (emailForm.contactEmail.trim().toLowerCase() === userInfo.contactEmail.trim().toLowerCase()) {
      setEmailError('')
      setEmailSuccess('The email is the same as the current one. No update needed.')
      return
    }

    setUpdatingEmail(true)
    setEmailError('')
    setEmailSuccess('')

    try {
      await userApi.updateEmail(userInfo.userID, emailForm.contactEmail)
      setEmailSuccess('Email updated successfully!')
      await loadUserInfo() // Reload to get updated info
    } catch (err: any) {
      setEmailError(err.response?.data?.error || 'Failed to update email')
    } finally {
      setUpdatingEmail(false)
    }
  }

  const handleUpdatePhone = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!userInfo) return

    // Normalize phone values for comparison (handle null/undefined/empty)
    const currentPhone = userInfo.contactPhone?.trim() || ''
    const newPhone = phoneForm.contactPhone?.trim() || ''

    // Check if the new phone is the same as the current phone
    if (newPhone === currentPhone) {
      setPhoneError('')
      setPhoneSuccess('The phone number is the same as the current one. No update needed.')
      return
    }

    setUpdatingPhone(true)
    setPhoneError('')
    setPhoneSuccess('')

    try {
      await userApi.updatePhone(userInfo.userID, phoneForm.contactPhone)
      setPhoneSuccess('Phone number updated successfully!')
      await loadUserInfo() // Reload to get updated info
    } catch (err: any) {
      setPhoneError(err.response?.data?.error || 'Failed to update phone number')
    } finally {
      setUpdatingPhone(false)
    }
  }

  const handleUpdatePassword = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!userInfo) return

    // Validate password confirmation before sending request
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setPasswordError('New passwords do not match')
      setPasswordSuccess('')
      return
    }

    // Additional validation: check if new password is not empty
    if (!passwordForm.newPassword || passwordForm.newPassword.trim() === '') {
      setPasswordError('New password cannot be empty')
      setPasswordSuccess('')
      return
    }

    // Check if the new password is the same as the old password
    if (passwordForm.oldPassword === passwordForm.newPassword) {
      setPasswordError('')
      setPasswordSuccess('The new password is the same as the current one. No update needed.')
      return
    }

    setUpdatingPassword(true)
    setPasswordError('')
    setPasswordSuccess('')

    try {
      await userApi.updatePassword(userInfo.userID, {
        contactEmail: userInfo.contactEmail,
        oldPassword: passwordForm.oldPassword,
        newPassword: passwordForm.newPassword,
      })
      setPasswordSuccess('Password updated successfully!')
      setPasswordForm({
        oldPassword: '',
        newPassword: '',
        confirmPassword: '',
      })
    } catch (err: any) {
      setPasswordError(err.response?.data?.error || 'Failed to update password')
    } finally {
      setUpdatingPassword(false)
    }
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-gray-600">Loading user information...</div>
      </div>
    )
  }

  if (!userInfo) {
    return (
      <div className="text-red-600">Failed to load user information</div>
    )
  }

  return (
    <div className="max-w-4xl">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Settings</h2>

      <div className="space-y-6">
        {/* Update Name Section */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Change Name</h3>
          {nameError && (
            <div className="mb-4 rounded-md bg-red-50 p-3">
              <div className="text-sm text-red-800">{nameError}</div>
            </div>
          )}
          {nameSuccess && (
            <div className="mb-4 rounded-md bg-green-50 p-3">
              <div className="text-sm text-green-800">{nameSuccess}</div>
            </div>
          )}
          <form onSubmit={handleUpdateName} className="space-y-4">
            <div>
              <label htmlFor="userName" className="block text-sm font-medium text-gray-700 mb-1">
                Full Name
              </label>
              <input
                id="userName"
                type="text"
                value={nameForm.userName}
                onChange={(e) => setNameForm({ userName: e.target.value })}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                placeholder="Enter your full name"
              />
            </div>
            <button
              type="submit"
              disabled={updatingName}
              className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {updatingName ? 'Updating...' : 'Update Name'}
            </button>
          </form>
        </div>

        {/* Update Email Section */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Change Email</h3>
          {emailError && (
            <div className="mb-4 rounded-md bg-red-50 p-3">
              <div className="text-sm text-red-800">{emailError}</div>
            </div>
          )}
          {emailSuccess && (
            <div className="mb-4 rounded-md bg-green-50 p-3">
              <div className="text-sm text-green-800">{emailSuccess}</div>
            </div>
          )}
          <form onSubmit={handleUpdateEmail} className="space-y-4">
            <div>
              <label htmlFor="contactEmail" className="block text-sm font-medium text-gray-700 mb-1">
                Email Address
              </label>
              <input
                id="contactEmail"
                type="email"
                value={emailForm.contactEmail}
                onChange={(e) => setEmailForm({ contactEmail: e.target.value })}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                placeholder="Enter your email address"
              />
            </div>
            <button
              type="submit"
              disabled={updatingEmail}
              className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {updatingEmail ? 'Updating...' : 'Update Email'}
            </button>
          </form>
        </div>

        {/* Update Phone Section */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Change Phone Number</h3>
          {phoneError && (
            <div className="mb-4 rounded-md bg-red-50 p-3">
              <div className="text-sm text-red-800">{phoneError}</div>
            </div>
          )}
          {phoneSuccess && (
            <div className="mb-4 rounded-md bg-green-50 p-3">
              <div className="text-sm text-green-800">{phoneSuccess}</div>
            </div>
          )}
          <form onSubmit={handleUpdatePhone} className="space-y-4">
            <div>
              <label htmlFor="contactPhone" className="block text-sm font-medium text-gray-700 mb-1">
                Phone Number
              </label>
              <input
                id="contactPhone"
                type="tel"
                value={phoneForm.contactPhone}
                onChange={(e) => setPhoneForm({ contactPhone: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                placeholder="Enter your phone number"
              />
            </div>
            <button
              type="submit"
              disabled={updatingPhone}
              className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {updatingPhone ? 'Updating...' : 'Update Phone'}
            </button>
          </form>
        </div>

        {/* Update Password Section */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Change Password</h3>
          {passwordError && (
            <div className="mb-4 rounded-md bg-red-50 p-3">
              <div className="text-sm text-red-800">{passwordError}</div>
            </div>
          )}
          {passwordSuccess && (
            <div className="mb-4 rounded-md bg-green-50 p-3">
              <div className="text-sm text-green-800">{passwordSuccess}</div>
            </div>
          )}
          <form onSubmit={handleUpdatePassword} className="space-y-4">
            <div>
              <label htmlFor="oldPassword" className="block text-sm font-medium text-gray-700 mb-1">
                Current Password
              </label>
              <div className="relative">
                <input
                  id="oldPassword"
                  type={showOldPassword ? 'text' : 'password'}
                  value={passwordForm.oldPassword}
                  onChange={(e) => setPasswordForm({ ...passwordForm, oldPassword: e.target.value })}
                  required
                  className="w-full px-3 py-2 pr-10 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                  placeholder="Enter your current password"
                />
                <button
                  type="button"
                  onClick={() => setShowOldPassword(!showOldPassword)}
                  className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-500 hover:text-gray-700"
                >
                  {showOldPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>
            <div>
              <label htmlFor="newPassword" className="block text-sm font-medium text-gray-700 mb-1">
                New Password
              </label>
              <div className="relative">
                <input
                  id="newPassword"
                  type={showNewPassword ? 'text' : 'password'}
                  value={passwordForm.newPassword}
                  onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })}
                  required
                  className="w-full px-3 py-2 pr-10 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                  placeholder="Enter your new password"
                />
                <button
                  type="button"
                  onClick={() => setShowNewPassword(!showNewPassword)}
                  className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-500 hover:text-gray-700"
                >
                  {showNewPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>
            <div>
              <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700 mb-1">
                Confirm New Password
              </label>
              <div className="relative">
                <input
                  id="confirmPassword"
                  type={showConfirmPassword ? 'text' : 'password'}
                  value={passwordForm.confirmPassword}
                  onChange={(e) => setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })}
                  required
                  className="w-full px-3 py-2 pr-10 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                  placeholder="Confirm your new password"
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-500 hover:text-gray-700"
                >
                  {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>
            <button
              type="submit"
              disabled={updatingPassword}
              className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {updatingPassword ? 'Updating...' : 'Update Password'}
            </button>
          </form>
        </div>
      </div>
    </div>
  )
}

export default Settings
