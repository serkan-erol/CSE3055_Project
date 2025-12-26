import { useState, useEffect } from 'react'
import { bankInfoApi, sessionApi } from '../../services/api'

interface BankInformation {
  sbiID?: number
  SBIID?: number // Handle both camelCase and PascalCase
  customerID?: number
  CustomerID?: number
  bankName?: string
  BankName?: string
  accountNo?: string
  AccountNo?: string
  iban?: string
  IBAN?: string
  createdAt?: string
  CreatedAt?: string
  lastUpdatedAt?: string
  LastUpdatedAt?: string
}

const BankInformation = () => {
  const [bankInfos, setBankInfos] = useState<BankInformation[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [customerId, setCustomerId] = useState<number | null>(null)

  const [formData, setFormData] = useState({
    bankName: '',
    accountNo: '',
    iban: '',
  })

  useEffect(() => {
    // Try to load from sessionStorage first
    const stored = sessionStorage.getItem('bankInfos')
    if (stored) {
      try {
        const parsed = JSON.parse(stored)
        if (Array.isArray(parsed) && parsed.length > 0) {
          setBankInfos(parsed)
          setLoading(false)
        }
      } catch (e) {
        console.error('Error parsing stored bank infos:', e)
      }
    }
    // Always fetch fresh data from API
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setLoading(true)
      setError('')
      const currentUser = await sessionApi.getCurrentUser()
      if (!currentUser.userId) {
        setError('Unable to identify customer. Please log in again.')
        return
      }
      setCustomerId(currentUser.userId)
      const infos = await bankInfoApi.getAllByCustomerId(currentUser.userId)
      // Use data as-is - ASP.NET Core should return camelCase
      const data = Array.isArray(infos) ? infos : []
      // Store in sessionStorage for persistence
      sessionStorage.setItem('bankInfos', JSON.stringify(data))
      setBankInfos(data)
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Failed to load bank information')
    } finally {
      setLoading(false)
    }
  }

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setFormData((prev) => ({ ...prev, [name]: value }))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!customerId) {
      setError('Unable to identify customer')
      return
    }

    // Validate that at least one field is provided
    if (!formData.bankName && !formData.accountNo && !formData.iban) {
      setError('Please provide at least one field (Bank Name, Account Number, or IBAN)')
      return
    }

    try {
      setError('')
      setSuccessMessage('')

      // Create new
      await bankInfoApi.create({
        customerID: customerId,
        bankName: formData.bankName || undefined,
        accountNo: formData.accountNo || undefined,
        iban: formData.iban || undefined,
      })
      setSuccessMessage('Bank information added successfully!')

      setShowForm(false)
      setFormData({ bankName: '', accountNo: '', iban: '' })
      await loadData() // This will update sessionStorage
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Failed to save bank information')
    }
  }

  const handleCancel = () => {
    setShowForm(false)
    setFormData({ bankName: '', accountNo: '', iban: '' })
    setError('')
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-gray-600">Loading bank information...</div>
      </div>
    )
  }

  return (
    <div className="max-w-4xl">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Bank Information</h2>
        {!showForm && (
          <button
            onClick={() => {
              setShowForm(true)
              setEditingId(null)
              setFormData({ bankName: '', accountNo: '', iban: '' })
              setError('')
            }}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
          >
            Add Bank Information
          </button>
        )}
      </div>

      {error && (
        <div className="mb-4 rounded-md bg-red-50 p-4">
          <div className="text-sm text-red-800">{error}</div>
        </div>
      )}

      {successMessage && (
        <div className="mb-4 rounded-md bg-green-50 p-4">
          <div className="text-sm text-green-800">{successMessage}</div>
        </div>
      )}

      {showForm && (
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Add New Bank Information
          </h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="bankName" className="block text-sm font-medium text-gray-700 mb-1">
                Bank Name
              </label>
              <input
                type="text"
                id="bankName"
                name="bankName"
                value={formData.bankName}
                onChange={handleInputChange}
                maxLength={50}
                placeholder="Enter bank name"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label htmlFor="accountNo" className="block text-sm font-medium text-gray-700 mb-1">
                Account Number
              </label>
              <input
                type="text"
                id="accountNo"
                name="accountNo"
                value={formData.accountNo}
                onChange={handleInputChange}
                maxLength={50}
                placeholder="Enter account number"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label htmlFor="iban" className="block text-sm font-medium text-gray-700 mb-1">
                IBAN
              </label>
              <input
                type="text"
                id="iban"
                name="iban"
                value={formData.iban}
                onChange={handleInputChange}
                maxLength={50}
                placeholder="Enter IBAN"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div className="text-sm text-gray-500">
              <p>Note: At least one field must be provided.</p>
            </div>

            <div className="flex gap-3">
              <button
                type="submit"
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
              >
                Add Bank Information
              </button>
              <button
                type="button"
                onClick={handleCancel}
                className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {bankInfos.length === 0 && !showForm ? (
        <div className="bg-white rounded-lg shadow p-8 text-center">
          <p className="text-gray-600 text-lg">No bank information saved yet.</p>
          <p className="text-gray-500 text-sm mt-2">Add bank information to get started.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {bankInfos.map((info: any, index) => {
            // Extract ID - handle both camelCase and PascalCase
            const sbiID = info.sbiID ?? info.SBIID ?? info.sbiId ?? info.SBIId
            const bankName = info.bankName ?? info.BankName
            const accountNo = info.accountNo ?? info.AccountNo
            const iban = info.iban ?? info.IBAN
            const createdAt = info.createdAt ?? info.CreatedAt
            
            if (!sbiID) {
              console.error('Bank info missing ID:', info)
            }
            
            return (
              <div key={sbiID ?? `bank-${index}`} className="bg-white rounded-lg shadow p-6">
                <div className="flex justify-between items-start">
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-gray-900 mb-3">
                      {bankName || 'Bank Account'}
                    </h3>
                    <div className="text-sm text-gray-600 space-y-2">
                      {bankName && (
                        <p>
                          <span className="font-medium">Bank Name:</span> {bankName}
                        </p>
                      )}
                      {accountNo && (
                        <p>
                          <span className="font-medium">Account Number:</span> {accountNo}
                        </p>
                      )}
                      {iban && (
                        <p>
                          <span className="font-medium">IBAN:</span> {iban}
                        </p>
                      )}
                      {createdAt && (
                        <p className="text-xs text-gray-500 mt-3">
                          <span className="font-medium">Added:</span> {formatDate(createdAt)}
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}

export default BankInformation

