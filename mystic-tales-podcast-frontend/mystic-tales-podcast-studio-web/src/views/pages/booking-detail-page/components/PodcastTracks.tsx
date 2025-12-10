// import { PlayArrow } from '@mui/icons-material';
// import { Box, Button, IconButton, Typography } from '@mui/material';
// import { CloudUpload, PlayArrow, Delete, Edit, CheckCircle, Cancel } from '@mui/icons-material';


// const PlayingAudio = () => {
//     return (
//         <Box>
//             <Typography sx={{ ...labelSx, fontSize: "0.85rem", mb: 2 }}>Podcast Tracks ({data.BookingPodcastTracks.length})</Typography>

//             <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
//                 {data.BookingPodcastTracks.map((track, index) => {
//                     const needsEdit = hasEditRequirement(track.Id);
//                     const editReqName = getEditRequirementName(track.Id);
//                     const reuploadFile = reuploadFiles[track.Id];

//                     return (
//                         <Box
//                             key={track.Id}
//                             sx={{
//                                 p: 2.5,
//                                 background: needsEdit
//                                     ? "linear-gradient(145deg, rgba(255, 152, 0, 0.08), rgba(255, 152, 0, 0.03))"
//                                     : "linear-gradient(145deg, rgba(174, 227, 57, 0.08), rgba(174, 227, 57, 0.03))",
//                                 borderRadius: "12px",
//                                 border: needsEdit
//                                     ? "1.5px solid rgba(255, 152, 0, 0.3)"
//                                     : "1px solid rgba(174, 227, 57, 0.2)",
//                                 transition: "all 0.3s ease",
//                             }}
//                         >
//                             <Box sx={{ display: "flex", alignItems: "flex-start", gap: 2, mb: 2 }}>
//                                 <Box
//                                     sx={{
//                                         width: 36,
//                                         height: 36,
//                                         background: needsEdit
//                                             ? "linear-gradient(135deg, #ff9800, #f57c00)"
//                                             : "linear-gradient(135deg, var(--primary-green), #7BA225)",
//                                         borderRadius: "10px",
//                                         display: "flex",
//                                         alignItems: "center",
//                                         justifyContent: "center",
//                                         color: "#000",
//                                         fontWeight: 700,
//                                         fontSize: "1rem",
//                                         flexShrink: 0,
//                                     }}
//                                 >
//                                     {index + 1}
//                                 </Box>
//                                 <Box sx={{ flex: 1 }}>
//                                     <Typography sx={{ color: "#fff", fontWeight: 600, fontSize: "0.95rem", mb: 0.5 }}>
//                                         {track.AudioFileKey}
//                                     </Typography>
//                                     <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
//                                         <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem" }}>
//                                             ⏱ {formatDuration(track.AudioLength)}
//                                         </Typography>
//                                         <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem" }}>
//                                             📦 {formatFileSize(track.AudioFileSize)}
//                                         </Typography>
//                                         <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.8rem" }}>
//                                             👁 {track.RemainingPreviewListenSlot} preview slots
//                                         </Typography>
//                                     </Box>
//                                 </Box>
//                                 {!needsEdit && (
//                                     <CheckCircle sx={{ color: "#4caf50", fontSize: "1.5rem" }} />
//                                 )}
//                             </Box>

//                             {needsEdit && (
//                                 <Box sx={{
//                                     mt: 2,
//                                     pt: 2,
//                                     borderTop: "1px solid rgba(255, 152, 0, 0.2)",
//                                     display: "flex",
//                                     flexDirection: "column",
//                                     gap: 1.5
//                                 }}>
//                                     <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
//                                         <Edit sx={{ color: "#ff9800", fontSize: "1rem" }} />
//                                         <Typography sx={{ color: "#ff9800", fontWeight: 600, fontSize: "0.85rem" }}>
//                                             Edit Required: {editReqName}
//                                         </Typography>
//                                     </Box>

//                                     {!reuploadFile ? (
//                                         <Button
//                                             variant="outlined"
//                                             component="label"
//                                             startIcon={<CloudUpload />}
//                                             sx={{
//                                                 color: "#ff9800",
//                                                 borderColor: "#ff9800",
//                                                 textTransform: "none",
//                                                 borderRadius: "10px",
//                                                 padding: "10px 20px",
//                                                 fontWeight: 600,
//                                                 "&:hover": {
//                                                     backgroundColor: "rgba(255, 152, 0, 0.1)",
//                                                     borderColor: "#ff9800",
//                                                 },
//                                             }}
//                                         >
//                                             Re-upload Audio
//                                             <input
//                                                 type="file"
//                                                 hidden
//                                                 accept="audio/*"
//                                                 onChange={(e) => {
//                                                     const file = e.target.files?.[0];
//                                                     if (file) handleFileChange(track.Id, file);
//                                                 }}
//                                             />
//                                         </Button>
//                                     ) : (
//                                         <Box sx={{
//                                             display: "flex",
//                                             justifyContent: "space-between",
//                                             alignItems: "center",
//                                             p: 1.5,
//                                             background: "rgba(255, 152, 0, 0.15)",
//                                             border: "1px solid rgba(255, 152, 0, 0.3)",
//                                             borderRadius: "8px"
//                                         }}>
//                                             <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
//                                                 <Box sx={{ fontSize: "1.5rem" }}>🎵</Box>
//                                                 <Box>
//                                                     <Typography sx={{ color: "#fff", fontSize: "0.85rem", fontWeight: 600 }}>
//                                                         {reuploadFile.name}
//                                                     </Typography>
//                                                     <Typography sx={{ color: "rgba(255,255,255,0.7)", fontSize: "0.75rem" }}>
//                                                         {formatFileSize(reuploadFile.size)}
//                                                     </Typography>
//                                                 </Box>
//                                             </Box>
//                                             <IconButton
//                                                 size="small"
//                                                 onClick={() => handleFileChange(track.Id, null)}
//                                                 sx={{ color: "#ef5350" }}
//                                             >
//                                                 <Delete fontSize="small" />
//                                             </IconButton>
//                                         </Box>
//                                     )}
//                                 </Box>
//                             )}

//                             <Box sx={{ mt: 2, display: "flex", gap: 1 }}>
//                                 <Button
//                                     size="small"
//                                     startIcon={<PlayArrow />}
//                                     sx={{
//                                         color: "var(--primary-green)",
//                                         textTransform: "none",
//                                         fontSize: "0.8rem",
//                                         "&:hover": { background: "rgba(174, 227, 57, 0.1)" }
//                                     }}
//                                 >
//                                     Preview
//                                 </Button>
//                             </Box>
//                         </Box>
//                     );
//                 })}
//             </Box>
//         </Box>
//     );
// };

// export default PlayingAudio;