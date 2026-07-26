const Footer = () => {
  return (
    <footer className="bg-white border-t border-gray-200 py-12 mt-12">
      <div className="max-w-6xl mx-auto px-4 grid md:grid-cols-3 gap-8 text-center md:text-left">
        <div>
          <h3 className="text-xl font-bold text-blue-600 mb-2">EventHub</h3>
          <p className="text-gray-500 text-sm">Vaša omiljena platforma za otkrivanje i organizaciju događaja.</p>
        </div>
        
        <div>
          <h4 className="font-bold text-gray-800 mb-4">Navigacija</h4>
          <ul className="text-gray-600 text-sm space-y-2">
            <li><a href="/events" className="hover:text-blue-600">Događaji</a></li>
            <li><a href="#" className="hover:text-blue-600">O nama</a></li>
            <li><a href="#" className="hover:text-blue-600">Kontakt</a></li>
          </ul>
        </div>

        <div>
          <h4 className="font-bold text-gray-800 mb-4">Pratite nas</h4>
          <p className="text-gray-500 text-sm">Instagram • Facebook • LinkedIn</p>
        </div>
      </div>
      <div className="text-center text-gray-400 text-xs mt-8">
        © 2026 EventHub. Sva prava zadržana.
      </div>
    </footer>
  );
};

export default Footer;