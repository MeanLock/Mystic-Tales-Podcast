import Swal from "sweetalert2";
import './styles.scss'
export const loginRequiredAlert = () => {
  return Swal.fire({
    title: `You haven't login yet`,
    text: "Please login to continue!",
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#3085d6',
    cancelButtonColor: '#d33',
    cancelButtonText: 'Ok',
    confirmButtonText: 'Login now',
  }).then((result) => {
    if (result.isConfirmed) {
      window.location.href = '/login';
    }
  })

};

// thông báo thành công với message
export const successAlert = (message: string) => {
  return Swal.fire({
    title: 'Success!',
    text: message,
    icon: 'success',
    confirmButtonColor: '#3085d6',
    cancelButtonColor: '#d33',
    confirmButtonText: 'OK',
  })
}

// thông báo lỗi với chỉ message
export const errorAlert = (message: string) => {
  return Swal.fire({
    title: 'Opps!',
    text: message,
    icon: 'error',
    confirmButtonColor: '#3085d6',
    cancelButtonColor: '#d33',
    confirmButtonText: 'OK',
  })
};

// thông báo hỏi confirm với message trả về boolean
export const confirmAlert = (message: string) => {
  return Swal.fire({
    html: `
      <div style="display:flex;flex-direction:column;gap:12px;">
        <p style="margin:0;font-size:15px;line-height:1.4;color:#ccc;">
          ${message}
        </p>
      </div>
    `,
    icon: 'warning',
    background: '#1c1c1c',
    color: '#ffffff',
    showCancelButton: true,
    buttonsStyling: false,
    customClass: {
      popup: 'swal2-dark-popup',
      icon: 'swal2-dark-icon',
      confirmButton: 'swal2-btn-confirm',
      cancelButton: 'swal2-btn-cancel'
    },
    confirmButtonText: 'Yes',
    cancelButtonText: 'No',
    reverseButtons: true,
    focusCancel: true
  });
};