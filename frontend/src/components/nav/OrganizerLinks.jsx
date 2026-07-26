import { Link } from 'react-router-dom';

const OrganizerLinks = () => {
  return (
    <>
      <Link to="/moji-dogadjaji" className="text-gray-600 hover:text-blue-600">
        Moji događaji
      </Link>
      <Link to="/dashboard/stats" className="text-gray-600 hover:text-blue-600">
        Statistika
      </Link>
      <Link to="/organizer-profile" className="text-gray-600 hover:text-blue-600">
        Moj profil
      </Link>
    </>
  );
};

export default OrganizerLinks;