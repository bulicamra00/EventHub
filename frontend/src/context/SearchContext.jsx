import { createContext, useState } from 'react';

export const SearchContext = createContext();

export const SearchProvider = ({ children }) => {
  const [searchTerm, setSearchTerm] = useState('');
  
  const [categoryId, setCategoryId] = useState(null);
  const [city, setCity] = useState('');
  const [status, setStatus] = useState(null);
  const [startDate, setStartDate] = useState(null);
  const [tagIds, setTagIds] = useState([]);
  const [onlyRecurring, setOnlyRecurring] = useState(false);
  
  const [location, setLocation] = useState({ 
    lat: null, 
    lon: null, 
    radius: 20 
  });

  const [sortBy, setSortBy] = useState('date'); 
  const [descending, setDescending] = useState(false);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const value = {
    searchTerm, 
    setSearchTerm,
    categoryId, 
    setCategoryId,
    city, 
    setCity,
    status, 
    setStatus,
    startDate, 
    setStartDate,
    tagIds, 
    setTagIds,
    onlyRecurring, 
    setOnlyRecurring,
    location, 
    setLocation,
    sortBy, 
    setSortBy,
    descending, 
    setDescending,
    pageNumber, 
    setPageNumber,
    pageSize, 
    setPageSize
  };

  return (
    <SearchContext.Provider value={value}>
      {children}
    </SearchContext.Provider>
  );
};