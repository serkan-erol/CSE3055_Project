import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { treasuryApi, sessionApi } from '../../services/api'

interface TreasuryEntry {
  treasuryID: number
  fTransactionID: number
  amount: number
  description: string | null
  entryDate: string
  lastUpdatedAt: string | null
}

interface TreasuryBalance {
  currentBalance: number
  lastUpdated: string
  totalEntries: number
  balanceAfter?: number
}

const ManageTreasury = () => {
  const navigate = useNavigate()
  const [employeeId, setEmployeeId] = useState<number | null>(null)
  const [accessLevel, setAccessLevel] = useState<number | null>(null)
  const [balance, setBalance] = useState<TreasuryBalance | null>(null)
  const [entries, setEntries] = useState<TreasuryEntry[]>([])
  const [entry, setEntry] = useState<TreasuryEntry | null>(null)
  const [searchType, setSearchType] = useState<'all' | 'treasury' | 'transaction'>('all')
  const [treasuryId, setTreasuryId] = useState('')
  const [fTransactionId, setFTransactionId] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [isLoadingBalance, setIsLoadingBalance] = useState(false)
  const [error, setError] = useState('')

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
          setAccessLevel(level)
          
          if (level < 4) {
            navigate('/employee/dashboard', {
              state: { message: 'You do not have permission to access this page. Access Level 4 or higher is required.' }
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
            setAccessLevel(userInfo.accessLevel)
            if (userInfo.accessLevel < 4) {
              navigate('/employee/dashboard', {
                state: { message: 'You do not have permission to access this page. Access Level 4 or higher is required.' }
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
    if (employeeId && accessLevel !== null && accessLevel >= 4) {
      loadBalance()
    }
  }, [employeeId, accessLevel])

  const loadBalance = async () => {
    if (!employeeId) return
    
    setIsLoadingBalance(true)
    try {
      const balanceData = await treasuryApi.getBalance(employeeId)
      setBalance(balanceData)
    } catch (err: any) {
      console.error('Error loading balance:', err)
    } finally {
      setIsLoadingBalance(false)
    }
  }

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!employeeId) {
      setError('Unable to search. Please refresh the page.')
      return
    }

    setError('')
    setEntry(null)
    setEntries([])
    setIsLoading(true)

    try {
      if (searchType === 'all') {
        const data = await treasuryApi.getAll(employeeId)
        setEntries(Array.isArray(data) ? data : [])
      } else if (searchType === 'treasury') {
        if (!treasuryId.trim()) {
          setError('Please enter a Treasury ID')
          setIsLoading(false)
          return
        }

        const parsedId = parseInt(treasuryId)
        if (isNaN(parsedId) || parsedId <= 0) {
          setError('Please enter a valid Treasury ID')
          setIsLoading(false)
          return
        }

        const data = await treasuryApi.getById(employeeId, parsedId)
        setEntry(data)
      } else {
        if (!fTransactionId.trim()) {
          setError('Please enter a Financial Transaction ID')
          setIsLoading(false)
          return
        }

        const parsedId = parseInt(fTransactionId)
        if (isNaN(parsedId) || parsedId <= 0) {
          setError('Please enter a valid Financial Transaction ID')
          setIsLoading(false)
          return
        }

        const data = await treasuryApi.getByTransactionId(employeeId, parsedId)
        // This might return an array or single entry
        if (Array.isArray(data)) {
          setEntries(data)
        } else {
          setEntry(data)
        }
      }
    } catch (err: any) {
      console.error('Error searching:', err)
      const errorMessage = err.response?.data?.error || err.response?.data?.message || err.message || 'Failed to search'
      setError(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: 'TRY',
    }).format(amount)
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

  const renderEntryDetails = (entryData: TreasuryEntry) => {
    return (
      <div className="bg-white rounded-lg shadow-md p-6">
        <h3 className="text-xl font-semibold text-gray-800 mb-6">Treasury Entry #{entryData.treasuryID}</h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-4">
            <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Entry Information</h4>
            <div className="space-y-2 text-gray-700">
              <p><strong>Treasury ID:</strong> {entryData.treasuryID}</p>
              <p><strong>Financial Transaction ID:</strong> {entryData.fTransactionID}</p>
              <p><strong>Entry Date:</strong> {formatDate(entryData.entryDate)}</p>
              {entryData.lastUpdatedAt && (
                <p><strong>Last Updated:</strong> {formatDate(entryData.lastUpdatedAt)}</p>
              )}
            </div>
          </div>

          <div className="space-y-4">
            <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Financial Details</h4>
            <div className="space-y-2 text-gray-700">
              <p><strong>Amount:</strong> {formatCurrency(entryData.amount)}</p>
              <p><strong>Description:</strong> {entryData.description || 'N/A'}</p>
            </div>
          </div>
        </div>
      </div>
    )
  }

  if (accessLevel === null || accessLevel < 4) {
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
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Manage Treasury</h2>

      {/* Current Balance */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <h3 className="text-lg font-semibold text-gray-800 mb-4">Current Balance</h3>
        {isLoadingBalance ? (
          <div className="text-gray-500">Loading balance...</div>
        ) : balance ? (
          <div className="space-y-2">
            <p className="text-3xl font-bold text-primary-600">
              {formatCurrency(balance.currentBalance || balance.balanceAfter || 0)}
            </p>
            <p className="text-sm text-gray-600">
              Last Updated: {formatDate(balance.lastUpdated)}
            </p>
            <p className="text-sm text-gray-600">
              Total Entries: {balance.totalEntries || entries.length}
            </p>
          </div>
        ) : (
          <div className="text-gray-500">Unable to load balance</div>
        )}
      </div>

      {/* Search Form */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <form onSubmit={handleSearch} className="space-y-4">
          <div className="flex gap-4 mb-4">
            <label className="flex items-center">
              <input
                type="radio"
                name="searchType"
                value="all"
                checked={searchType === 'all'}
                onChange={(e) => {
                  setSearchType('all')
                  setEntry(null)
                  setEntries([])
                  setError('')
                }}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-700">View All Entries</span>
            </label>
            <label className="flex items-center">
              <input
                type="radio"
                name="searchType"
                value="treasury"
                checked={searchType === 'treasury'}
                onChange={(e) => {
                  setSearchType('treasury')
                  setEntry(null)
                  setEntries([])
                  setError('')
                }}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-700">Search by Treasury ID</span>
            </label>
            <label className="flex items-center">
              <input
                type="radio"
                name="searchType"
                value="transaction"
                checked={searchType === 'transaction'}
                onChange={(e) => {
                  setSearchType('transaction')
                  setEntry(null)
                  setEntries([])
                  setError('')
                }}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-700">Search by Transaction ID</span>
            </label>
          </div>

          {searchType !== 'all' && (
            <div className="flex gap-4">
              {searchType === 'treasury' ? (
                <div className="flex-1">
                  <label htmlFor="treasuryId" className="block text-sm font-medium text-gray-700 mb-2">
                    Treasury ID
                  </label>
                  <input
                    id="treasuryId"
                    type="text"
                    value={treasuryId}
                    onChange={(e) => setTreasuryId(e.target.value)}
                    placeholder="Enter treasury ID"
                    className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                    disabled={isLoading}
                  />
                </div>
              ) : (
                <div className="flex-1">
                  <label htmlFor="fTransactionId" className="block text-sm font-medium text-gray-700 mb-2">
                    Financial Transaction ID
                  </label>
                  <input
                    id="fTransactionId"
                    type="text"
                    value={fTransactionId}
                    onChange={(e) => setFTransactionId(e.target.value)}
                    placeholder="Enter transaction ID"
                    className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                    disabled={isLoading}
                  />
                </div>
              )}
              <div className="flex items-end">
                <button
                  type="submit"
                  disabled={isLoading}
                  className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isLoading ? 'Searching...' : 'Search'}
                </button>
              </div>
            </div>
          )}

          {searchType === 'all' && (
            <div className="flex justify-end">
              <button
                type="submit"
                disabled={isLoading}
                className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? 'Loading...' : 'Load All Entries'}
              </button>
            </div>
          )}
        </form>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4 mb-6">
          <div className="text-sm font-medium text-red-800">Error: {error}</div>
        </div>
      )}

      {/* Single Entry */}
      {entry && renderEntryDetails(entry)}

      {/* Multiple Entries */}
      {entries.length > 0 && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-gray-800">
            {searchType === 'all' ? 'All Treasury Entries' : `Found ${entries.length} entry/entries`}
          </h3>
          <div className="bg-white rounded-lg shadow-md overflow-hidden">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">FT ID</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Amount</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Description</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Entry Date</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {entries.map((entryData) => (
                    <tr key={entryData.treasuryID} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{entryData.treasuryID}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{entryData.fTransactionID}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{formatCurrency(entryData.amount)}</td>
                      <td className="px-6 py-4 text-sm text-gray-500">{entryData.description || 'N/A'}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatDate(entryData.entryDate)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {!entry && entries.length === 0 && !error && !isLoading && (
        <div className="bg-white rounded-lg shadow-md p-12 text-center">
          <p className="text-gray-500 text-lg">
            {searchType === 'all' 
              ? 'Click "Load All Entries" to view all treasury entries'
              : searchType === 'treasury'
              ? 'Enter a Treasury ID above to view details'
              : 'Enter a Financial Transaction ID above to view entries'}
          </p>
        </div>
      )}
    </div>
  )
}

export default ManageTreasury

