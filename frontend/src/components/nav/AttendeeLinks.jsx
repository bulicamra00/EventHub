import { Link } from 'react-router-dom';
import NotificationBell from '../NotificationBell';

const AttendeeLinks = () => {
  return (
    <>
      <Link to="/moje-ulaznice" className="text-gray-600 hover:text-blue-600">
        Moje ulaznice
      </Link>
      <Link to="/profile" className="text-gray-600 hover:text-blue-600">
        Moj profil
      </Link>
      <NotificationBell />
    </>
  );
};

export default AttendeeLinks;