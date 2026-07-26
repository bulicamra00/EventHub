import { Link } from 'react-router-dom';

const AdminLinks = () => {
  return (
    <>
      <Link to="/admin/categories" className="text-gray-600 hover:text-blue-600">
        Kategorije
      </Link>
      <Link to="/admin/reports" className="text-gray-600 hover:text-blue-600">
        Prijave
      </Link>
      <Link to="/admin/organizer-requests" className="text-gray-600 hover:text-blue-600">
        Zahtevi
      </Link>
      <Link to="/admin/stats" className="text-gray-600 hover:text-blue-600">
        Statistika platforme
      </Link>
    </>
  );
};

export default AdminLinks;