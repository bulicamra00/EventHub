import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { SearchProvider } from './context/SearchContext';
import Home from './pages/Home';
import Events from './pages/Events';
import Organizers from './pages/Organizers';
import OrganizerDetails from './pages/OrganizerDetails';
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';
import VerifyEmail from './pages/auth/VerifyEmail';
import EventDetails from './pages/events/EventDetails';

import ProtectedRoute from './components/ProtectedRoute';
import MyTickets from './pages/tickets/MyTickets';
import Checkout from './pages/bookings/Checkout';
import UserProfile from './pages/profile/UserProfile'; 
import OrganizerProfile from './pages/profile/OrganizerProfile'; 
import MyEvents from './pages/organizer/MyEvents';
import CreateEvent from './pages/organizer/CreateEvent';
import EditEvent from './pages/organizer/EditEvent';
import OrganizerDashboard from './pages/organizer/OrganizerDashboard';
import EventManagement from './pages/organizer/EventManagement';


import CategoriesManagement from './pages/admin/CategoriesManagement';
import ReportsAndUsers from './pages/admin/ReportsAndUsers';
import OrganizerRequests from './pages/admin/OrganizerRequests';
import PlatformStats from './pages/admin/PlatformStats'; 

function App() {
  return (
    <SearchProvider>
      <Router>
        <Toaster position="top-right" /> 
        
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/events" element={<Events />} />
          <Route path="/organizers" element={<Organizers />} />
          <Route path="/organizers/:id" element={<OrganizerDetails />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/verify-email" element={<VerifyEmail />} />
          <Route path="/events/:id" element={<EventDetails />} />
          
          <Route 
            path="/moje-ulaznice" 
            element={
              <ProtectedRoute allowedRole="Attendee">
                <MyTickets />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/checkout/:bookingId" 
            element={
              <ProtectedRoute allowedRole="Attendee">
                <Checkout />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/profile" 
            element={
              <ProtectedRoute allowedRole="Attendee">
                <UserProfile />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/organizer-profile" 
            element={
              <ProtectedRoute allowedRole="Organizer">
                <OrganizerProfile />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/moji-dogadjaji" 
            element={
              <ProtectedRoute allowedRole="Organizer">
                <MyEvents />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/kreiraj-dogadjaj" 
            element={
              <ProtectedRoute allowedRole="Organizer">
                <CreateEvent />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/izmeni-dogadjaj/:id" 
            element={
              <ProtectedRoute allowedRole="Organizer">
                <EditEvent />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/upravljanje-dogadjajem/:id" 
            element={
              <ProtectedRoute allowedRole="Organizer">
                <EventManagement />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/dashboard/stats" 
            element={
              <ProtectedRoute allowedRole="Organizer">
                <OrganizerDashboard />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/admin/categories" 
            element={
              <ProtectedRoute allowedRole="Admin">
                <CategoriesManagement />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/admin/reports" 
            element={
              <ProtectedRoute allowedRole="Admin">
                <ReportsAndUsers />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/admin/organizer-requests" 
            element={
              <ProtectedRoute allowedRole="Admin">
                <OrganizerRequests />
              </ProtectedRoute>
            } 
          />

          <Route 
            path="/admin/stats" 
            element={
              <ProtectedRoute allowedRole="Admin">
                <PlatformStats />
              </ProtectedRoute>
            } 
          />
        </Routes>
      </Router>
    </SearchProvider>
  );
}

export default App;