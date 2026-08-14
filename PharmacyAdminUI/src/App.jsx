import React, { useState, useEffect } from 'react';
import { Search, Link, Trash2, X, AlertCircle, Phone } from 'lucide-react';
import api, { getUnmatchedPharmacies, getPharmacies, matchPharmacy, deleteUnmatchedPharmacy } from './api';
import './App.css';

function App() {
  const [unmatched, setUnmatched] = useState([]);
  const [loading, setLoading] = useState(true);
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedUnmatched, setSelectedUnmatched] = useState(null);
  
  const [searchQuery, setSearchQuery] = useState('');
  const [realPharmacies, setRealPharmacies] = useState([]);
  const [filteredPharmacies, setFilteredPharmacies] = useState([]);
  
  useEffect(() => {
    fetchUnmatched();
    fetchRealPharmacies();
  }, []);
  
  const fetchUnmatched = async () => {
    try {
      setLoading(true);
      const data = await getUnmatchedPharmacies();
      setUnmatched(data);
    } catch (error) {
      console.error("Error fetching unmatched pharmacies", error);
    } finally {
      setLoading(false);
    }
  };
  
  const fetchRealPharmacies = async () => {
    try {
      const data = await getPharmacies();
      setRealPharmacies(data);
    } catch (error) {
      console.error("Error fetching real pharmacies", error);
    }
  };
  
  useEffect(() => {
    if (!searchQuery || searchQuery.trim() === '') {
      setFilteredPharmacies([]);
      return;
    }
    
    const lowerQuery = searchQuery.toLowerCase();
    const filtered = realPharmacies.filter(p => 
      (p.name && p.name.toLowerCase().includes(lowerQuery)) || 
      (p.districtName && p.districtName.toLowerCase().includes(lowerQuery)) ||
      (p.phoneNumber && p.phoneNumber.includes(lowerQuery))
    );
    setFilteredPharmacies(filtered.slice(0, 15));
  }, [searchQuery, realPharmacies]);

  const handleOpenMatchModal = (pharmacy) => {
    setSelectedUnmatched(pharmacy);
    let cleanName = (pharmacy.scrapedName || '').replace(/eczan.*/i, '').trim();
    setSearchQuery(cleanName);
    setIsModalOpen(true);
  };
  
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedUnmatched(null);
    setSearchQuery('');
  };
  
  const handleMatch = async (realPharmacyId) => {
    try {
      await matchPharmacy(selectedUnmatched.id, realPharmacyId);
      alert("Basariyla eslestirildi!");
      handleCloseModal();
      fetchUnmatched();
    } catch (error) {
      alert("Eslestirme sirasinda hata olustu.");
      console.error(error);
    }
  };
  
  const handleDelete = async (id) => {
    if (!window.confirm("Bu karantina kaydini silmek istediginize emin misiniz?")) return;
    
    try {
      await deleteUnmatchedPharmacy(id);
      fetchUnmatched();
    } catch (error) {
      alert("Silme sirasinda hata olustu.");
      console.error(error);
    }
  };
  
  const getInsuranceName = (val) => {
    if (!val) return "Bilinmiyor";
    
    const insurances = {
      1: "Allianz",
      2: "Türkiye Sigorta",
      3: "Mapfre Sigorta",
      4: "Eureko Sigorta",
      5: "Bupa Acıbadem",
      6: "Axa Sigorta",
      7: "Anadolu Sigorta",
      8: "Aksigorta"
    };
    return insurances[val] || val;
  };

  return (
    <div className="app-container">
      <header>
        <h1>PharmacyMatch</h1>
        <p>Eşleşmeyen Eczaneleri (Karantina) Yönetim Paneli</p>
      </header>

      <main className="glass-panel table-container">
        {loading ? (
          <div className="empty-state">Yükleniyor...</div>
        ) : unmatched.length === 0 ? (
          <div className="empty-state">
            <AlertCircle size={48} style={{ margin: '0 auto 16px', opacity: 0.5 }} />
            <p>Harika! Tüm eczaneler başarıyla eşleştirildi.</p>
          </div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Kazınan İsim</th>
                <th>Adres & Telefon</th>
                <th>Kaynak Sigorta</th>
                <th>Tarih</th>
                <th>Aksiyonlar</th>
              </tr>
            </thead>
            <tbody>
              {unmatched.map(item => (
                <tr key={item.id}>
                  <td><strong>{item.scrapedName}</strong></td>
                  <td>
                    <div style={{ fontSize: '0.9rem' }}>{item.scrapedAddress || '-'}</div>
                    {item.scrapedPhoneNumber && (
                      <div style={{ fontSize: '0.8rem', color: 'var(--primary-color)', marginTop: '4px', display: 'flex', alignItems: 'center', gap: '4px' }}>
                        <Phone size={12} /> {item.scrapedPhoneNumber}
                      </div>
                    )}
                  </td>
                  <td>
                    {item.sourceInsurance || item.dataSource ? (
                      <span className="badge insurance">{getInsuranceName(item.sourceInsurance) || item.dataSource}</span>
                    ) : (
                      <span className="badge">Web</span>
                    )}
                  </td>
                  <td>{new Date(item.createdAt).toLocaleDateString('tr-TR')}</td>
                  <td>
                    <div className="actions">
                      <button className="btn-primary" onClick={() => handleOpenMatchModal(item)}>
                        <Link size={16} /> Eşleştir
                      </button>
                      <button className="btn-danger" onClick={() => handleDelete(item.id)}>
                        <Trash2 size={16} /> Sil
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </main>

      {/* Match Modal */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="glass-panel modal-content" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Eczane Eşleştir</h2>
              <button className="close-btn" onClick={handleCloseModal}>
                <X size={24} />
              </button>
            </div>
            
            <div style={{ marginBottom: '20px' }}>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '8px' }}>
                Aranan (Karantinadaki) Eczane:
              </p>
              <div style={{ padding: '12px', background: 'rgba(59, 130, 246, 0.1)', border: '1px solid rgba(59, 130, 246, 0.3)', borderRadius: '8px' }}>
                <strong style={{ color: '#60a5fa' }}>{selectedUnmatched?.scrapedName}</strong>
                <div style={{ fontSize: '0.8rem', marginTop: '4px', opacity: 0.8 }}>{selectedUnmatched?.scrapedAddress}</div>
                {selectedUnmatched?.scrapedPhoneNumber && (
                  <div style={{ fontSize: '0.85rem', color: 'var(--primary-color)', marginTop: '6px', display: 'flex', alignItems: 'center', gap: '4px', fontWeight: 'bold' }}>
                    <Phone size={14} /> {selectedUnmatched.scrapedPhoneNumber}
                  </div>
                )}
              </div>
            </div>

            <div className="search-box">
              <Search className="search-icon" size={20} />
              <input 
                type="text" 
                placeholder="Gerçek eczanelerde ara (örn: Şifa veya İlçe veya Telefon)..." 
                value={searchQuery}
                onChange={e => setSearchQuery(e.target.value)}
                autoFocus
              />
            </div>

            <div className="pharmacy-list">
              {filteredPharmacies.length === 0 ? (
                <div className="empty-state">Sonuç bulunamadı. Lütfen daha belirgin bir kelime arayın.</div>
              ) : (
                filteredPharmacies.map(real => (
                  <div key={real.id} className="pharmacy-card">
                    <div className="pharmacy-info">
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <h3>{real.name}</h3>
                        {real.districtName && (
                          <span style={{ fontSize: '0.75rem', padding: '2px 8px', background: 'rgba(255,255,255,0.1)', borderRadius: '12px' }}>
                            {real.districtName}
                          </span>
                        )}
                      </div>
                      <p style={{ marginTop: '4px' }}>{real.address}</p>
                      {real.phoneNumber && (
                        <div style={{ fontSize: '0.85rem', color: 'var(--primary-color)', marginTop: '4px', display: 'flex', alignItems: 'center', gap: '4px', fontWeight: 'bold' }}>
                          <Phone size={14} /> {real.phoneNumber}
                        </div>
                      )}
                    </div>
                    <button className="btn-primary" onClick={() => handleMatch(real.id)}>
                      Seç
                    </button>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
