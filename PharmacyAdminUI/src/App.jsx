import React, { useState, useEffect } from 'react';
import { Search, Link, Trash2, X, AlertCircle, Phone, LogOut, PlusCircle, Users } from 'lucide-react';
import api, { getUnmatchedPharmacies, getPharmacies, matchPharmacy, deleteUnmatchedPharmacy, approveAsNewPharmacy, login, changeRole, getSuggestions } from './api';
import './App.css';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('token'));
  const [userRole, setUserRole] = useState(null);
  
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loginError, setLoginError] = useState('');
  const [loginLoading, setLoginLoading] = useState(false);

  const [activeTab, setActiveTab] = useState('karantina');

  const [unmatched, setUnmatched] = useState([]);
  const [loading, setLoading] = useState(true);
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedUnmatched, setSelectedUnmatched] = useState(null);
  
  const [searchQuery, setSearchQuery] = useState('');
  const [suggestedPharmacies, setSuggestedPharmacies] = useState([]);
  const [filteredPharmacies, setFilteredPharmacies] = useState([]);
  const [modalLoading, setModalLoading] = useState(false);
  
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [addPharmacyData, setAddPharmacyData] = useState({
    id: null,
    name: '',
    address: '',
    phoneNumber: '',
    latitude: '',
    longitude: ''
  });
  
  const decodeJwt = (token) => {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      return JSON.parse(jsonPayload);
    } catch (e) {
      return null;
    }
  };

  useEffect(() => {
    if (isAuthenticated) {
      const token = localStorage.getItem('token');
      if (token) {
        const decoded = decodeJwt(token);
        if (decoded) setUserRole(decoded.role);
      }
      fetchUnmatched();
    }
  }, [isAuthenticated]);
  
  const fetchUnmatched = async () => {
    try {
      setLoading(true);
      const data = await getUnmatchedPharmacies();
      setUnmatched(data.data || data.Data || data.items || data || []);
    } catch (error) {
      console.error("Error fetching unmatched pharmacies", error);
    } finally {
      setLoading(false);
    }
  };
  
  
  
  useEffect(() => {
    if (!searchQuery || searchQuery.trim() === '') {
      setFilteredPharmacies(suggestedPharmacies);
      return;
    }
    
    const lowerQuery = searchQuery.toLowerCase();
    const filtered = suggestedPharmacies.filter(p => 
      (p.name && p.name.toLowerCase().includes(lowerQuery)) || 
      (p.districtName && p.districtName.toLowerCase().includes(lowerQuery)) ||
      (p.phoneNumber && p.phoneNumber.includes(lowerQuery))
    );
    setFilteredPharmacies(filtered);
  }, [searchQuery, suggestedPharmacies]);

  const handleOpenMatchModal = async (pharmacy) => {
    setSelectedUnmatched(pharmacy);
    setSearchQuery('');
    setIsModalOpen(true);
    
    setModalLoading(true);
    try {
      const suggestions = await getSuggestions(pharmacy.id);
      setSuggestedPharmacies(suggestions.data || suggestions || []);
    } catch (error) {
      console.error(error);
      setSuggestedPharmacies([]);
    } finally {
      setModalLoading(false);
    }
  };
  
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedUnmatched(null);
    setSearchQuery('');
  };
  
  const handleMatch = async (realPharmacyId) => {
    try {
      await matchPharmacy(selectedUnmatched.id, realPharmacyId);
      alert("Başarıyla eşleştirildi!");
      handleCloseModal();
      fetchUnmatched();
    } catch (error) {
      alert("Eşleştirme sırasında hata oluştu.");
      console.error(error);
    }
  };
  
  const handleDelete = async (id) => {
    if (!window.confirm("Bu karantina kaydını silmek istediğinize emin misiniz?")) return;
    
    try {
      await deleteUnmatchedPharmacy(id);
      fetchUnmatched();
    } catch (error) {
      alert("Silme sırasında hata oluştu.");
      console.error(error);
    }
  };

  const handleOpenAddModal = (pharmacy) => {
    setAddPharmacyData({
      id: pharmacy.id,
      name: pharmacy.scrapedName || '',
      address: pharmacy.scrapedAddress || '',
      phoneNumber: pharmacy.scrapedPhoneNumber || '',
      latitude: pharmacy.scrapedLatitude || '',
      longitude: pharmacy.scrapedLongitude || ''
    });
    setIsAddModalOpen(true);
  };

  const handleApproveAsNew = async () => {
    try {
      const payload = {
        name: addPharmacyData.name,
        address: addPharmacyData.address,
        phoneNumber: addPharmacyData.phoneNumber,
        latitude: addPharmacyData.latitude ? parseFloat(addPharmacyData.latitude) : null,
        longitude: addPharmacyData.longitude ? parseFloat(addPharmacyData.longitude) : null
      };
      await approveAsNewPharmacy(addPharmacyData.id, payload);
      alert("Başarıyla yeni eczane olarak eklendi!");
      setIsAddModalOpen(false);
      fetchUnmatched();
    } catch (error) {
      alert(error.response?.data?.message || "Ekleme sırasında hata oluştu.");
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

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      setLoginLoading(true);
      setLoginError('');
      const data = await login(email, password);
      if (data.accessToken) {
        const decoded = decodeJwt(data.accessToken);
        if (decoded && decoded.role === 'User') {
          setLoginError('Bu panele sadece yöneticiler (Admin/SuperAdmin) giriş yapabilir.');
          setLoginLoading(false);
          return;
        }
        localStorage.setItem('token', data.accessToken);
        setIsAuthenticated(true);
      }
    } catch (error) {
      setLoginError('Giriş başarısız. Lütfen bilgilerinizi kontrol edin.');
    } finally {
      setLoginLoading(false);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    setIsAuthenticated(false);
    setUserRole(null);
  };

  if (!isAuthenticated) {
    return (
      <div className="login-container">
        <div className="glass-panel login-panel">
          <h2>Admin Girişi</h2>
          {loginError && <div className="error-message">{loginError}</div>}
          <form onSubmit={handleLogin}>
            <div className="form-group">
              <label>E-posta</label>
              <input 
                type="email" 
                value={email} 
                onChange={(e) => setEmail(e.target.value)} 
                required 
              />
            </div>
            <div className="form-group">
              <label>Şifre</label>
              <input 
                type="password" 
                value={password} 
                onChange={(e) => setPassword(e.target.value)} 
                required 
              />
            </div>
            <button type="submit" className="btn-primary" disabled={loginLoading} style={{ width: '100%', marginTop: '1rem' }}>
              {loginLoading ? 'Giriş Yapılıyor...' : 'Giriş Yap'}
            </button>
          </form>
        </div>
      </div>
    );
  }

  return (
    <div className="app-container">
      <header>
        <div>
          <h1>PharmacyMatch Admin</h1>
          <p>Eczane ve Sistem Yönetim Paneli</p>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          {userRole && <span style={{ color: 'var(--text-secondary)' }}>Yetki: <strong>{userRole}</strong></span>}
          <button className="btn-danger" onClick={handleLogout} style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <LogOut size={16} /> Çıkış Yap
          </button>
        </div>
      </header>

      {userRole === 'SuperAdmin' && (
        <div className="tabs">
          <button 
            className={`tab-btn ${activeTab === 'karantina' ? 'active' : ''}`}
            onClick={() => setActiveTab('karantina')}
          >
            Karantina
          </button>
          <button 
            className={`tab-btn ${activeTab === 'admin_yonetimi' ? 'active' : ''}`}
            onClick={() => setActiveTab('admin_yonetimi')}
          >
            <Users size={16} /> Admin Yönetimi
          </button>
        </div>
      )}

      {activeTab === 'karantina' && (
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
                        <button className="btn-primary" onClick={() => handleOpenMatchModal(item)} style={{ background: '#3b82f6', borderColor: '#3b82f6' }}>
                          <Link size={16} /> Eşleştir
                        </button>
                        <button 
                          className="btn btn-secondary btn-sm"
                          onClick={() => handleOpenAddModal(item)}
                        >
                          <PlusCircle size={16} className="btn-icon" />
                          Yeni Olarak Ekle
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
      )}

      {activeTab === 'admin_yonetimi' && <ManageAdminsComponent />}

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
              {modalLoading ? (
                <div className="empty-state">Öneriler yükleniyor... Lütfen bekleyin.</div>
              ) : filteredPharmacies.length === 0 ? (
                <div className="empty-state">Sonuç bulunamadı.</div>
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

      {isAddModalOpen && (
        <div className="modal-overlay" style={{ zIndex: 10000 }}>
          <div className="modal-content add-modal-content">
            <div className="modal-header">
              <h2>Yeni Eczane Olarak Ekle</h2>
              <button className="close-btn" onClick={() => setIsAddModalOpen(false)}>
                <X size={24} />
              </button>
            </div>
            
            <div className="add-modal-body" style={{ padding: '20px' }}>
              <div style={{ marginBottom: '15px' }}>
                <label style={{ display: 'block', marginBottom: '5px', color: '#fff' }}>Eczane Adı</label>
                <input 
                  type="text" 
                  style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #444', background: '#222', color: '#fff' }}
                  value={addPharmacyData.name} 
                  onChange={e => setAddPharmacyData({...addPharmacyData, name: e.target.value})} 
                />
              </div>
              <div style={{ marginBottom: '15px' }}>
                <label style={{ display: 'block', marginBottom: '5px', color: '#fff' }}>Adres</label>
                <textarea 
                  style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #444', minHeight: '80px', background: '#222', color: '#fff' }}
                  value={addPharmacyData.address} 
                  onChange={e => setAddPharmacyData({...addPharmacyData, address: e.target.value})} 
                />
              </div>
              <div style={{ marginBottom: '15px' }}>
                <label style={{ display: 'block', marginBottom: '5px', color: '#fff' }}>Telefon</label>
                <input 
                  type="text" 
                  style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #444', background: '#222', color: '#fff' }}
                  value={addPharmacyData.phoneNumber} 
                  onChange={e => setAddPharmacyData({...addPharmacyData, phoneNumber: e.target.value})} 
                />
              </div>
              <div style={{ display: 'flex', gap: '15px', marginBottom: '20px' }}>
                <div style={{ flex: 1 }}>
                  <label style={{ display: 'block', marginBottom: '5px', color: '#fff' }}>Enlem (Latitude)</label>
                  <input 
                    type="number" 
                    step="any"
                    placeholder="Örn: 41.0082"
                    style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #444', background: '#222', color: '#fff' }}
                    value={addPharmacyData.latitude} 
                    onChange={e => setAddPharmacyData({...addPharmacyData, latitude: e.target.value})} 
                  />
                </div>
                <div style={{ flex: 1 }}>
                  <label style={{ display: 'block', marginBottom: '5px', color: '#fff' }}>Boylam (Longitude)</label>
                  <input 
                    type="number" 
                    step="any"
                    placeholder="Örn: 28.9784"
                    style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #444', background: '#222', color: '#fff' }}
                    value={addPharmacyData.longitude} 
                    onChange={e => setAddPharmacyData({...addPharmacyData, longitude: e.target.value})} 
                  />
                </div>
              </div>
              
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
                <button className="btn" style={{ backgroundColor: '#444', color: '#fff' }} onClick={() => setIsAddModalOpen(false)}>
                  İptal
                </button>
                <button className="btn btn-primary" onClick={handleApproveAsNew}>
                  Kaydet ve Onayla
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}

function ManageAdminsComponent() {
  const [email, setEmail] = useState('');
  const [newRole, setNewRole] = useState(1);
  const [loading, setLoading] = useState(false);
  const [msg, setMsg] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await changeRole(email, newRole);
      setMsg(res.message || 'Yetki başarıyla güncellendi!');
      setEmail('');
    } catch (error) {
      setMsg(error.response?.data?.message || error.response?.data || 'Güncelleme sırasında bir hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="glass-panel" style={{ padding: '30px', maxWidth: '600px', margin: '0 auto' }}>
      <h2>Admin Yetki Yönetimi</h2>
      {msg && <div style={{ marginBottom: '16px', color: 'var(--primary-color)' }}>{msg}</div>}
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Kullanıcı E-posta</label>
          <input type="email" value={email} onChange={e => setEmail(e.target.value)} required />
        </div>
        <div className="form-group">
          <label>Yeni Rol</label>
          <select 
            value={newRole} 
            onChange={e => setNewRole(parseInt(e.target.value))}
            style={{ width: '100%', padding: '12px', background: 'var(--bg-dark)', color: 'var(--text-primary)', border: '1px solid var(--border-color)', borderRadius: '8px' }}
          >
            <option value={1}>Admin</option>
            <option value={3}>SuperAdmin</option>
            <option value={2}>User</option>
          </select>
        </div>
        <button className="btn-primary" type="submit" disabled={loading} style={{ marginTop: '10px' }}>
          {loading ? 'Güncelleniyor...' : 'Yetkiyi Güncelle'}
        </button>
      </form>
    </div>
  );
}

export default App;
