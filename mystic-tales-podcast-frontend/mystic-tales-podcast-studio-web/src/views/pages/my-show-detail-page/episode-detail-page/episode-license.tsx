import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { deleteLicense, getLicenseFile, getLicenses, getLicenseTypes, uploadLicense } from '@/core/services/episode/episode.service';
import { LicenseFile, LicenseType } from '@/core/types';
import { EmptyComponent } from '@/views/components/common/empty';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import { DocumentViewer } from '@/views/components/common/document';
import { Add, Close, CloudUpload, Delete, Visibility } from '@mui/icons-material';
import { Box, Button, IconButton, MenuItem, TextField, Typography, Dialog, DialogTitle, DialogContent, Modal } from '@mui/material';
import { useContext, useEffect, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { on } from 'events';
import { confirmAlert } from '@/core/utils/alert.util';
import { set } from 'lodash';
import Loading from '@/views/components/common/loading';
import { EpisodeDetailViewContext } from '.';
import * as signalR from "@microsoft/signalr";
import { useSelector } from 'react-redux';
import { RootState } from '@/redux/rootReducer';


const EpisodeLicense = () => {
    const { episodeId } = useParams<{ episodeId: string }>();
    const ctx = useContext(EpisodeDetailViewContext);
    const authSlice = ctx?.authSlice;
    const episodeDetail = ctx?.episodeDetail;
    const refreshEpisode = ctx?.refreshEpisode;
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [licenses, setLicenses] = useState<LicenseFile[]>([]);
    const [licenseTypes, setLicenseTypes] = useState<LicenseType[]>([]);
    const [newLicenses, setNewLicenses] = useState<{
        id: string;
        file: File | null;
        licenseTypeId: number;
        fileName: string;
    }[]>([
        {
            id: Date.now().toString(),
            file: null,
            licenseTypeId: 1,
            fileName: '',
        },
    ]);
    const [viewingLicenseFile, setViewingLicenseFile] = useState<{ id: string, url: string } | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [uploading, setUploading] = useState<boolean>(false);
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })


    const fecthLicenses = async () => {
        setLoading(true);
        try {
            const res = await getLicenses(loginRequiredAxiosInstance, episodeId);
            console.log("Fetched license :", res.data.PodcastEpisodeLicenseList);
            if (res.success && res.data) {
                setLicenses(res.data.PodcastEpisodeLicenseList || []);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch licenses:', error);
        } finally {
            setLoading(false);
        }
    }
    const fecthLicenseTypes = async () => {
        setLoading(true);
        try {
            const res = await getLicenseTypes(loginRequiredAxiosInstance);
            console.log("Fetched license types :", res.data.PodcastEpisodeLicenseTypeList);
            if (res.success && res.data) {
                setLicenseTypes(res.data.PodcastEpisodeLicenseTypeList);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch license types:', error);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        fecthLicenses()
        fecthLicenseTypes()
    }, [episodeId]);


    const handleAddNewLicense = () => {
        const newLicense = {
            id: Date.now().toString(),
            file: null,
            licenseTypeId: licenseTypes[0]?.Id || 1,
            fileName: ''
        };
        setNewLicenses(prev => [...prev, newLicense]);
    };

    const handleViewFile = async (fileId: string, fileKey: string) => {
        try {
            const response = await getLicenseFile(loginRequiredAxiosInstance, fileKey)
            if (response.success && response.data) {
                setViewingLicenseFile({ id: fileId, url: response.data.FileUrl })
            }
        } catch (error) {
            console.error('Error fetching PDF:', error)
            toast.error('Failed to load PDF')
        }
    }

    const handleDownloadLicense = async (fileKey: string, fileName: string) => {
        try {
            const response = await getLicenseFile(loginRequiredAxiosInstance, fileKey)
            if (response.success && response.data) {
                const link = document.createElement('a')
                link.href = response.data.FileUrl
                link.download = fileName
                link.target = '_blank'
                document.body.appendChild(link)
                link.click()
                document.body.removeChild(link)
            }
        } catch (error) {
            console.error('Error downloading file:', error)
            toast.error('Failed to download file')
        }
    }

    const handleRemoveNewLicense = (id: string) => {
        setNewLicenses(prev => prev.filter(license => license.id !== id));
    };

    const handleNewLicenseTypeChange = (id: string, typeId: number) => {
        setNewLicenses(prev => prev.map(license =>
            license.id === id ? { ...license, licenseTypeId: typeId } : license
        ));
    };

    const handleNewLicenseFileChange = (id: string, file: File | null) => {
        if (!file) return;

        const validExtensions = ['pdf', 'doc', 'docx'];
        const fileExtension = file.name.split('.').pop()?.toLowerCase();
        if (!fileExtension || !validExtensions.includes(fileExtension)) {
            toast.error('Invalid file type. Please upload PDF, DOC, or DOCX files only.');
            return;
        }

        const maxSize = 50 * 1024 * 1024;
        if (file.size > maxSize) {
            toast.error('File size exceeds 50MB limit.');
            return;
        }

        setNewLicenses(prev => prev.map(license =>
            license.id === id ? { ...license, file, fileName: file.name } : license
        ));
    };

    const prepareLicenseFilesForUpload = (): File[] => {
        return newLicenses
            .filter(license => license.file !== null)
            .map(license => {
                const file = license.file!;
                const extension = file.name.split('.').pop();
                const newFileName = `license_${license.licenseTypeId}.${extension}`;

                return new File([file], newFileName, { type: file.type });
            });
    };

    const handleUploadLicenses = async () => {
               if (authSlice.user?.ViolationLevel > 0) {
                    toast.error('Your account is currently under violation !!');
                    return;
                }
        try {
            setUploading(true);
            const filesToUpload = prepareLicenseFilesForUpload();

            if (filesToUpload.length === 0) {
                toast.error('Please add at least one license file.');
                return;
            }
            console.log("Uploading files:", filesToUpload);
            const res = await uploadLicense(loginRequiredAxiosInstance, episodeId, {
                LicenseDocumentFiles: filesToUpload
            });

            const sagaId = res?.data?.SagaInstanceId;
            if (!res.success && res.message.content) {
                toast.error(res.message.content);
                return;
            }
            if (!sagaId) {
                toast.error('Upload license failed, please try again.');
                return;
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    toast.success('Licenses uploaded successfully.');
                    setNewLicenses([]);
                    fecthLicenses();
                },
                onFailure: (err) => toast.error(err || 'Saga failed!'),
                onTimeout: () => toast.error('System not responding, please try again.'),
            });
        } catch (error) {
            toast.error('Error uploading licenses');
        } finally {
            setUploading(false);
        }
    };
    const handleRemoveLicense = async (licenseId: string) => {
        const alert = await confirmAlert("Are you sure to DELETE this license?");
        if (!alert.isConfirmed) return;
        setLoading(true);
        try {
            const res = await deleteLicense(loginRequiredAxiosInstance, episodeId, licenseId);
            const sagaId = res?.data?.SagaInstanceId;
            if (!res.success && res.message.content) {
                toast.error(res.message.content);
                return;
            }
            if (!sagaId) {
                toast.error('Delete license failed, please try again.');
                return;
            }

            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    toast.success('License deleted successfully.');
                    fecthLicenses();
                },
                onFailure: (err) => toast.error(err || 'Saga failed!'),
                onTimeout: () => toast.error('System not responding, please try again.'),
            });
        } catch (error) {
            toast.error('Error deleting licenses');
        } finally {
            setLoading(false);
        }
    };
    const connectionRef = useRef<signalR.HubConnection | null>(null);
        const authSlice2 = useSelector((state: RootState) => state.auth);
    
        const token = authSlice2.token || "";
        const REST_API_BASE_URL = import.meta.env.VITE_BACKEND_URL;
    
        useEffect(() => {
            // Build connection
            console.log("Setting up SignalR connection...", token);
            const connection = new signalR.HubConnectionBuilder()
                .withUrl(`${REST_API_BASE_URL}/api/podcast-service/hubs/podcast-content-notification`, {
                    accessTokenFactory: () => {
                        return token;
                    }
                })
                .withAutomaticReconnect()
                .build();
    
            connectionRef.current = connection;
    
            // Register events
            connection.on("PodcastEpisodeAudioProcessingCompletedNotification", async (data) => {
                console.log("Audio processing :", data);
    
                if (!data.IsSuccess) {
                    console.error("Audio processing failed:", data.ErrorMessage);
                    return;
                }
    
                //alert(`Audio processing completed for Podcast ID: ${data}`);
                await refreshEpisode?.();
            });
    
            // Start connection
            connection.start()
                .then(() => console.log("SignalR connected"))
                .catch(err => console.error("SignalR connection error:", err));
    
            // Cleanup
            return () => {
                connection.stop();
            };
        }, []);
    if (loading) {
        return (
            <div className="flex justify-center items-center h-100">
                <Loading />
            </div>
        )
    }
    if (licenses.length === 0) {
        return (
            <>
                <EmptyComponent item="Licenses" subtitle="No licenses added yet. Click 'Add License' to upload a license document." />
                <div className="flex justify-center mt-6">
                    <Modal_Button
                        className="episode-info-page__license-btn--contained h-1/2 text-black font-bold rounded-lg normal-case "
                        content="Add License"
                        variant="contained"
                        size="md"
                        startIcon={<Add />}
                    >
                        <div className="flex flex-col gap-4 p-8">
                            {newLicenses.map((license) => (
                                <Box
                                    key={license.id}
                                    sx={{
                                        backgroundColor: 'rgba(255, 255, 255, 0.05)',
                                        padding: '1rem',
                                        borderRadius: '8px',
                                        border: '1px solid rgba(255, 255, 255, 0.1)'
                                    }}
                                >
                                    <div className="flex gap-3 items-start">
                                        <div className="flex-1 flex flex-col gap-3">
                                            <TextField
                                                select
                                                label="License Type"
                                                value={license.licenseTypeId}
                                                onChange={(e) => handleNewLicenseTypeChange(license.id, Number(e.target.value))}
                                                size="small"
                                                fullWidth
                                                variant="standard"
                                                sx={{
                                                    '& .MuiInputBase-root': {
                                                        marginTop: '1.475rem',
                                                        color: 'white',
                                                        '&:before': { borderColor: 'rgba(255, 255, 255, 0.1)' },
                                                        '&:hover:not(.Mui-disabled):before': { borderColor: 'rgba(255, 255, 255, 0.2)' },
                                                        '&:after': { borderColor: 'var(--primary-green)' }
                                                    },
                                                    '& .MuiInputLabel-root': { color: 'var(--primary-green)' },
                                                    '& .MuiSelect-icon': { color: 'rgba(255, 255, 255, 0.5)' }
                                                }}
                                            >
                                                {licenseTypes.map((type) => (
                                                    <MenuItem key={type.Id} value={type.Id}>
                                                        {type.Name}
                                                    </MenuItem>
                                                ))}
                                            </TextField>
                                            <Box
                                                onClick={() => fileInputRef.current?.click()}
                                                sx={{
                                                    border: '2px dashed rgba(255, 255, 255, 0.2)',
                                                    borderRadius: '8px',
                                                    padding: '1.5rem',
                                                    textAlign: 'center',
                                                    cursor: 'pointer',
                                                    transition: 'all 0.3s ease',
                                                    backgroundColor: license.file ? 'rgba(76, 175, 80, 0.05)' : 'transparent',
                                                    '&:hover': {
                                                        borderColor: 'var(--primary-green)',
                                                        backgroundColor: 'rgba(76, 175, 80, 0.1)'
                                                    }
                                                }}
                                            >
                                                <Typography sx={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '0.875rem' }}>
                                                    {license.fileName || 'Click to select file'}
                                                </Typography>
                                                <Typography variant="caption" sx={{ color: 'rgba(255, 255, 255, 0.4)', fontSize: '0.7rem' }}>
                                                    PDF, DOC, DOCX (Max 50MB)
                                                </Typography>
                                            </Box>
                                            <input
                                                ref={fileInputRef}
                                                type="file"
                                                accept=".pdf,.doc,.docx"
                                                onChange={(e) => handleNewLicenseFileChange(license.id, e.target.files?.[0] || null)}
                                                style={{ display: 'none' }}
                                            />
                                        </div>
                                        <IconButton
                                            size="small"
                                            onClick={() => handleRemoveNewLicense(license.id)}
                                            sx={{
                                                color: 'rgba(255, 255, 255, 0.5)',
                                                '&:hover': {
                                                    color: '#ff4444',
                                                    backgroundColor: 'rgba(255, 68, 68, 0.1)'
                                                }
                                            }}
                                        >
                                            <Delete fontSize="small" />
                                        </IconButton>
                                    </div>
                                </Box>
                            ))}
                            <Button
                                variant="outlined"
                                startIcon={<Add />}
                                onClick={handleAddNewLicense}
                                sx={{
                                    borderColor: 'rgba(255, 255, 255, 0.2)',
                                    color: 'white',
                                    textTransform: 'none',
                                    '&:hover': {
                                        borderColor: 'rgba(255, 255, 255, 0.4)',
                                        backgroundColor: 'rgba(255, 255, 255, 0.05)'
                                    }
                                }}
                            >
                                Add More License
                            </Button>
                            <div className="flex gap-3 justify-end mt-4">
                                <Button
                                    variant="contained"
                                    onClick={handleUploadLicenses}
                                    disabled={uploading || newLicenses.every(l => !l.file)}
                                    sx={{
                                        backgroundColor: 'var(--primary-green)',
                                        color: 'black',
                                        '&:hover': { backgroundColor: '#8bc34a' },
                                        '&:disabled': { backgroundColor: 'rgba(255, 255, 255, 0.12)' }
                                    }}
                                >
                                    {uploading ? 'Uploading...' : 'Upload Licenses'}
                                </Button>
                            </div>
                        </div>
                    </Modal_Button>

                </div>

            </>
        );
    }


    return (
        <div className="px-10 py-12">
            <div className="episode-info-page__license-section">
                <div className="flex items-center justify-between mb-4">
                    <Typography variant="h6" sx={{ color: 'white', fontSize: '1rem', fontWeight: 600 }}>
                        License Documents
                    </Typography>
                    <Modal_Button
                        className="episode-info-page__license-btn h-1/2 text-black font-bold rounded-lg normal-case "
                        content="Add License"
                        variant="outlined"
                        size="md"
                        startIcon={<Add />}
                    >
                        <div className="flex flex-col gap-4 p-8">
                            {newLicenses.map((license) => (
                                <Box
                                    key={license.id}
                                    sx={{
                                        backgroundColor: 'rgba(255, 255, 255, 0.05)',
                                        padding: '1rem',
                                        borderRadius: '8px',
                                        border: '1px solid rgba(255, 255, 255, 0.1)'
                                    }}
                                >
                                    <div className="flex gap-3 items-start">
                                        <div className="flex-1 flex flex-col gap-3">
                                            <TextField
                                                select
                                                label="License Type"
                                                value={license.licenseTypeId}
                                                onChange={(e) => handleNewLicenseTypeChange(license.id, Number(e.target.value))}
                                                size="small"
                                                fullWidth
                                                variant="standard"
                                                sx={{
                                                    '& .MuiInputBase-root': {
                                                        color: 'white',
                                                        '&:before': { borderColor: 'rgba(255, 255, 255, 0.1)' },
                                                        '&:hover:not(.Mui-disabled):before': { borderColor: 'rgba(255, 255, 255, 0.2)' },
                                                        '&:after': { borderColor: 'var(--primary-green)' }
                                                    },
                                                    '& .MuiInputLabel-root': { color: 'var(--primary-green)' },
                                                    '& .MuiSelect-icon': { color: 'rgba(255, 255, 255, 0.5)' }
                                                }}
                                            >
                                                {licenseTypes.map((type) => (
                                                    <MenuItem key={type.Id} value={type.Id}>
                                                        {type.Name}
                                                    </MenuItem>
                                                ))}
                                            </TextField>
                                            <Box
                                                onClick={() => fileInputRef.current?.click()}
                                                sx={{
                                                    border: '2px dashed rgba(255, 255, 255, 0.2)',
                                                    borderRadius: '8px',
                                                    padding: '1.5rem',
                                                    textAlign: 'center',
                                                    cursor: 'pointer',
                                                    transition: 'all 0.3s ease',
                                                    backgroundColor: license.file ? 'rgba(76, 175, 80, 0.05)' : 'transparent',
                                                    '&:hover': {
                                                        borderColor: 'var(--primary-green)',
                                                        backgroundColor: 'rgba(76, 175, 80, 0.1)'
                                                    }
                                                }}
                                            >
                                                <Typography sx={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '0.875rem' }}>
                                                    {license.fileName || 'Click to select file'}
                                                </Typography>
                                                <Typography variant="caption" sx={{ color: 'rgba(255, 255, 255, 0.4)', fontSize: '0.7rem' }}>
                                                    PDF, DOC, DOCX (Max 50MB)
                                                </Typography>
                                            </Box>
                                            <input
                                                ref={fileInputRef}
                                                type="file"
                                                accept=".pdf,.doc,.docx"
                                                onChange={(e) => handleNewLicenseFileChange(license.id, e.target.files?.[0] || null)}
                                                style={{ display: 'none' }}
                                            />
                                        </div>
                                        <IconButton
                                            size="small"
                                            onClick={() => handleRemoveNewLicense(license.id)}
                                            sx={{
                                                color: 'rgba(255, 255, 255, 0.5)',
                                                '&:hover': {
                                                    color: '#ff4444',
                                                    backgroundColor: 'rgba(255, 68, 68, 0.1)'
                                                }
                                            }}
                                        >
                                            <Delete fontSize="small" />
                                        </IconButton>
                                    </div>
                                </Box>
                            ))}
                            <Button
                                variant="outlined"
                                startIcon={<Add />}
                                onClick={handleAddNewLicense}
                                sx={{
                                    borderColor: 'rgba(255, 255, 255, 0.2)',
                                    color: 'white',
                                    textTransform: 'none',
                                    '&:hover': {
                                        borderColor: 'rgba(255, 255, 255, 0.4)',
                                        backgroundColor: 'rgba(255, 255, 255, 0.05)'
                                    }
                                }}
                            >
                                Add More License
                            </Button>
                            <div className="flex gap-3 justify-end mt-4">
                                <Button
                                    variant="contained"
                                    onClick={handleUploadLicenses}
                                    disabled={uploading || newLicenses.every(l => !l.file)}
                                    sx={{
                                        backgroundColor: 'var(--primary-green)',
                                        color: 'black',
                                        '&:hover': { backgroundColor: '#8bc34a' },
                                        '&:disabled': { backgroundColor: 'rgba(255, 255, 255, 0.12)' }
                                    }}
                                >
                                    {uploading ? 'Uploading...' : 'Upload Licenses'}
                                </Button>
                            </div>
                        </div>
                    </Modal_Button>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {licenses.map((license, index) => (
                        <Box
                            key={license.Id}
                            sx={{
                                boxShadow: '0 9px 9.4px rgba(0, 0, 0, 0.2)',
                                backgroundColor: 'rgba(61, 59, 59, 0.09)',
                                backdropFilter: 'blur(10px)',
                                padding: '1rem',
                                borderRadius: '12px',
                                transition: 'all 0.3s ease',
                                display: 'flex',
                                flexDirection: 'column',
                                height: '100%',
                                '&:hover': {
                                    backgroundColor: 'rgba(255, 255, 255, 0.08)',
                                    borderColor: 'rgba(255, 255, 255, 0.2)',
                                    transform: 'translateY(-2px)'
                                }
                            }}
                        >
                            <div className="flex flex-col gap-3 h-full">
                                {/* <div className="flex justify-center">
                                    <Box
                                        sx={{
                                            border: '1px solid var(--primary-green)',
                                            borderRadius: '8px',
                                            padding: '1rem',
                                            display: 'flex',
                                            alignItems: 'center',
                                            justifyContent: 'center',
                                            width: '80px',
                                            height: '80px'
                                        }}
                                    >
                                        <Typography sx={{ color: 'var(--primary-green)', fontSize: '2.5rem' }}>
                                            📄
                                        </Typography>
                                    </Box>
                                </div> */}
                                <div className="text-center flex-1">
                                    <Typography
                                        variant="body2"
                                        sx={{
                                            color: 'rgba(255, 255, 255, 0.5)',
                                            fontSize: '0.7rem',
                                            mb: 0.5
                                        }}
                                    >
                                        License {index + 1}
                                    </Typography>
                                    <Typography
                                        variant="body1"
                                        sx={{
                                            color: 'white',
                                            fontSize: '0.85rem',
                                            fontWeight: 500,
                                            lineHeight: 1.4
                                        }}
                                    >
                                        {license.PodcastEpisodeLicenseType.Name}
                                    </Typography>
                                </div>
                                <div className="flex justify-center gap-1 pt-2 border-t border-white/10">
                                    <IconButton
                                        size="small"
                                        onClick={() => handleViewFile(license.Id, license.LicenseDocumentFileKey)}
                                        sx={{
                                            color: 'var(--primary-green)',
                                            '&:hover': { backgroundColor: 'rgba(76, 175, 80, 0.1)' }
                                        }}
                                    >
                                        <Visibility fontSize="small" />
                                    </IconButton>
                                    <IconButton
                                        size="small"
                                        onClick={() => handleDownloadLicense(license.LicenseDocumentFileKey, `license_${license.PodcastEpisodeLicenseType.Name}.pdf`)}
                                        sx={{
                                            color: 'rgba(255, 255, 255, 0.7)',
                                            '&:hover': { backgroundColor: 'rgba(255, 255, 255, 0.1)' }
                                        }}
                                    >
                                        <CloudUpload fontSize="small" />
                                    </IconButton>
                                    <IconButton
                                        size="small"
                                        onClick={() => handleRemoveLicense(license.Id)}
                                        sx={{
                                            color: '#ff4444',
                                            '&:hover': {
                                                color: '#ff4444',
                                                backgroundColor: 'rgba(255, 68, 68, 0.1)'
                                            }
                                        }}
                                    >
                                        <Delete fontSize="small" />
                                    </IconButton>
                                </div>
                            </div>
                        </Box>
                    ))}
                </div>
            </div>

            <Dialog
                open={!!viewingLicenseFile}
                onClose={() => setViewingLicenseFile(null)}
                maxWidth="md"
                fullWidth
                PaperProps={{
                    sx: {
                        backgroundColor: '#1a1a1a',
                        color: 'white',
                        minHeight: '500px'
                    }
                }}
            >
                <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="h6">License Document Preview</Typography>
                    <IconButton onClick={() => setViewingLicenseFile(null)} sx={{ color: 'white' }}>
                        <Close />
                    </IconButton>
                </DialogTitle>
                <DialogContent>
                    {viewingLicenseFile && (
                        <DocumentViewer url={viewingLicenseFile.url} height={600} />
                    )}
                </DialogContent>
            </Dialog>


        </div>
    );
};

export default EpisodeLicense;


