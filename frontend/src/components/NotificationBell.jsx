import { useState, useEffect } from 'react';
import { FaBell } from 'react-icons/fa';
import api from '../api';

const NotificationBell = () => {
  const [notifications, setNotifications] = useState([]);
  const [isOpen, setIsOpen] = useState(false);

  const fetchNotifications = async () => {
    try {
      const response = await api.get('/notifications/my');
      setNotifications(response.data);
    } catch (err) {
      console.error("Greška pri učitavanju notifikacija", err);
    }
  };

  useEffect(() => {
    fetchNotifications();
  }, []);

  const handleMarkAsRead = async (id) => {
    const notification = notifications.find(n => n.id === id);
    if (notification?.isRead) return;

    try {
      await api.post(`/notifications/read/${id}`);
      
      setNotifications((prev) => 
        prev.map((n) => n.id === id ? { ...n, isRead: true } : n)
      );
    } catch (err) {
      console.error("Greška pri obeležavanju notifikacije", err);
    }
  };

  const unreadCount = notifications.filter(n => !n.isRead).length;

  return (
    <div className="relative">
      <button 
        onClick={() => setIsOpen(!isOpen)} 
        className="text-gray-600 hover:text-blue-600 relative transition-colors"
      >
        <FaBell size={20} />
        {unreadCount > 0 && (
          <span className="absolute -top-2 -right-2 bg-red-500 text-white text-[10px] w-5 h-5 flex items-center justify-center rounded-full font-bold">
            {unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-3 w-80 bg-white border border-gray-200 rounded-xl shadow-2xl z-100 overflow-hidden">
          <div className="p-3 border-b bg-gray-50">
            <h3 className="font-bold text-gray-700">Notifikacije</h3>
          </div>
          
          <div className="max-h-80 overflow-y-auto">
            {notifications.length === 0 ? (
              <p className="p-6 text-center text-gray-500 text-sm">Nema obaveštenja.</p>
            ) : (
              notifications.map((n) => (
                <div 
                  key={n.id} 
                  onClick={() => handleMarkAsRead(n.id)}
                  className={`p-4 border-b border-gray-100 transition-colors cursor-pointer 
                    ${n.isRead ? 'bg-white opacity-60' : 'bg-blue-50 hover:bg-blue-100'}`}
                >
                  <p className={`text-sm leading-snug ${n.isRead ? 'text-gray-500' : 'text-gray-800 font-semibold'}`}>
                    {n.message}
                  </p>
                  <span className="text-[11px] text-gray-400 block mt-1">
                    {new Date(n.createdAt).toLocaleString()}
                  </span>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default NotificationBell;