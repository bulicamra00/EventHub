import { useState, useEffect, useContext } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import { SearchContext } from '../context/SearchContext';
import AttendeeLinks from './nav/AttendeeLinks';
import OrganizerLinks from './nav/OrganizerLinks';
import AdminLinks from './nav/AdminLinks'; 

const Navbar = () => {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const { searchTerm, setSearchTerm, setLocation } = useContext(SearchContext);

  const handleGetLocation = () => {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          setLocation({
            lat: pos.coords.latitude,
            lon: pos.coords.longitude,
            radius: 100 
          });
          navigate('/events'); 
        },
        (err) => {
          alert("Nismo uspeli da dobijemo lokaciju: " + err.message);
        }
      );
    } else {
      alert("Geolokacija nije podržana u tvom pretraživaču.");
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      navigate('/events');
    }
  };

  const checkUser = () => {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decoded = jwtDecode(token);
        setUser({
          role: decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        });
      } catch (error) {
        setUser(null);
      }
    } else {
      setUser(null);
    }
  };

  useEffect(() => {
    checkUser();
    window.addEventListener('authChanged', checkUser);
    return () => window.removeEventListener('authChanged', checkUser);
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('token');
    setUser(null);
    window.dispatchEvent(new Event('authChanged'));
    window.location.href = '/';
  };

  return (
    <nav className="flex items-center justify-between p-4 bg-white shadow-md">
      <Link to="/" className="text-2xl font-bold text-blue-600">EventHub</Link>
      
      <div className="flex gap-6 items-center">
        {user?.role !== 'Organizer' && user?.role !== 'Admin' && (
          <>
            <Link to="/" className="text-gray-600 hover:text-blue-600">Početna</Link>
            <Link to="/events" className="text-gray-600 hover:text-blue-600">Događaji</Link>
            <Link to="/organizers" className="text-gray-600 hover:text-blue-600">Organizatori</Link>
          </>
        )}

        {user?.role === 'Attendee' && <AttendeeLinks />}
        {user?.role === 'Organizer' && <OrganizerLinks />}
        {user?.role === 'Admin' && <AdminLinks />}
        
        {user?.role !== 'Organizer' && user?.role !== 'Admin' && (
          <div className="flex items-center gap-2">
            <button 
              onClick={handleGetLocation}
              className="text-sm bg-blue-100 text-blue-700 px-3 py-1 rounded hover:bg-blue-200 transition"
              title="Prikaži događaje blizu mene"
            >
              📍 Blizu mene
            </button>

            <input 
              type="text" 
              placeholder="Pretraži..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onKeyDown={handleKeyDown}
              className="border border-gray-300 rounded px-3 py-1 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        )}

        {user ? (
          <button 
            onClick={handleLogout}
            className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
          >
            Odjavi se
          </button>
        ) : (
          <Link to="/login" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
            Uloguj se
          </Link>
        )}
      </div>
    </nav>
  );
};

export default Navbar;