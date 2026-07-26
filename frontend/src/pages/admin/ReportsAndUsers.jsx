import { useState } from 'react';
import Navbar from '../../components/Navbar';
import ReportUser from '../../components/admin/ReportUser';
import ReportEvent from '../../components/admin/ReportEvent';

const ReportsAndUsers = () => {
  const [activeTab, setActiveTab] = useState('users'); 

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="container mx-auto p-6 max-w-5xl">
        <h1 className="text-3xl font-bold text-gray-800 mb-6">Administracija platforme</h1>

        <div className="flex border-b border-gray-200 mb-6">
          <button
            onClick={() => setActiveTab('users')}
            className={`py-2 px-6 font-semibold transition border-b-2 ${
              activeTab === 'users'
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Upravljanje korisnicima
          </button>
          <button
            onClick={() => setActiveTab('events')}
            className={`py-2 px-6 font-semibold transition border-b-2 ${
              activeTab === 'events'
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Prijave i moderacija događaja
          </button>
        </div>

        <div>
          {activeTab === 'users' && <ReportUser />}
          {activeTab === 'events' && <ReportEvent />}
        </div>
      </div>
    </div>
  );
};

export default ReportsAndUsers;