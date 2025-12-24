import Swal from "sweetalert2";

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
    text: message,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#3085d6',
    cancelButtonColor: '#d33',
    confirmButtonText: 'Yes',
    cancelButtonText: 'No',
  })
};