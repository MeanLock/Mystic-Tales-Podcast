import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Eye } from 'phosphor-react';
import Modal_Button from '../../../components/common/modal/ModalButton';
import { Account } from '../../../../core/types';
import { adminAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2';
import SurveyTalkLoading from '../../../components/common/loading';
import AvatarInput from '../../../components/common/avatar';
import { getManagerAccounts } from '../../../../core/services/account/account.service';
import ManagerUpdate from './ManagerUpdate';
import { formatDate } from '../../../../core/utils/date.util';
import { callGraphQLMutation, callGraphQLQuery } from '@/core/api/graphql/main/api-call';
import { publicApolloClient } from '@/core/api/graphql/config/clients/v2';

ModuleRegistry.registerModules([AllCommunityModule]);

interface ManagerViewProps { }
interface ManagerViewContextProps {
  handleDataChange: () => void;
}
interface GridState {
  columnDefs: ColDef[];
  rowData: Account[];
}

export const ManagerViewContext = createContext<ManagerViewContextProps | null>(null);

const state_creator = (table: Account[]) => {
  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.4 },
      {
        headerName: "Avatar", flex: 0.5,
        cellRenderer: (params: any) => {
          return (
            <AvatarInput size={50} src={params.data.MainImageUrl} />)

        },
      },
      { headerName: "Họ và Tên", field: "FullName" },
      {
        headerName: "Email", field: "Email",
      },
      {
        headerName: "Số Điện Thoại", field: "Phone",
      },
      {
        headerName: "Ngày Tham Gia",
        valueGetter: (params: { data: Account }) => formatDate(params.data.CreatedAt),
      },
      {
        headerName: "Trạng thái",
        cellClass: 'd-flex align-items-center',
        cellRenderer: (params: any) => {
          const status = params.data.DeactivatedAt !== null
            ? { title: 'Deactivated', color: 'danger' }
            : { title: 'Activate', color: 'success' };

          return (
            <CCard
              textColor={`${status.color}`}
              style={{ width: '100px' }}
              className={`text-center fw-bold rounded-pill px-1 border-2 border-${status.color} bg-light`}
            >
              {status.title}
            </CCard>
          );
        },
      },
      {
        headerName: "Option",
        cellClass: 'd-flex justify-content-center py-0',
        cellRenderer: (params: { data: Account }) => {
          const Modal_props = {
            updateForm: <ManagerUpdate account={params.data} onClose={() => { }} />,
            title: 'Update Manager [ID: #' + params.data.Id + ']',
            button: <Eye size={27} color="purple" weight="duotone" />,
            update_button_color: 'white'
          }
          return (

            <CButtonGroup style={{ width: '100%', height: "100%" }} role="group" aria-label="Basic mixed styles example">
              <Modal_Button
                disabled={false}
                title={Modal_props.title}
                content={Modal_props.button}
                color={Modal_props.update_button_color} >
                {Modal_props.updateForm}
              </Modal_Button>
            </CButtonGroup>
          )

        },
        flex: 0.5,
      }
    ],
    rowData: table

  }
  return state
}

const ManagerView: FC<ManagerViewProps> = () => {
  let [state, setState] = useState<GridState | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const handleDataChange = async () => {
    setIsLoading(true);
    try {
      const accountList = await getManagerAccounts(adminAxiosInstance);
      if (accountList.success) {
        setState(state_creator(accountList.data.Accounts));
      } else {
        console.error('API Error:', accountList.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch Manager accounts:', error);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    handleDataChange()
  }, [])
  const defaultColDef = useMemo(() => {
    return {
      flex: 1,
      filter: true,
      autoHeight: true,
      resizable: true,
      wrapText: true,
      cellClass: 'd-flex align-items-center',
      editable: false
    };
  }, [])
  //==========TEST API GRAPHQL===========
  const [products, setProducts] = useState([]);
  const fetchProducts = async () => {
    const filters = {
      minPrice: 1090,
      maxPrice: 2000,
      sortBy: {
        price: "DESC"
      }
    };

    const productList = await getAllProducts(filters);
    console.log('Product List:', productList);
    if (productList.success) {
      setProducts(productList.data._dbQueries._productQueries.allProducts);
    } else {
      console.error('API Error:', productList.errorMessage);
    }
  }
  useEffect(() => {
    fetchProducts();
  }, [])
  const handleAddProduct = async () => {
    try {
      const newProduct = {
        productName: "New Product",
        categoryId: 1,
        price: 2000,
        stockQuantity: 100
      };

      const response = await addProduct(newProduct);

      if (response.success) {
        console.log('Product added:', response.data._dbMutations._productMutations.addProduct);
        fetchProducts();
      } else {
        console.error('Failed to add product:', response.errorMessage);
      }
    } catch (error) {
      console.error('Error adding product:', error);
    }
  };
  const handleDeleteProduct = async () => {
    try {
      const productId = 502;
      const response = await deleteProduct(productId);

      if (response.success) {
        console.log('Product deleted:', response.data._dbMutations._productMutations.deleteProduct);
        fetchProducts();
      } else {
        console.error('Failed to delete product:', response.errorMessage);
      }
    } catch (error) {
      console.error('Error deleting product:', error);
    }
  };
  const handleUpdateProduct = async () => {
    try {
      const updatedProduct = {
        productId: 500,
        productName: "Thịnh Test Update",
        price: 1999,
        stockQuantity: 150
      };

      const response = await updateProduct(updatedProduct);

      if (response.success) {
        console.log('Product updated:', response.data._dbMutations._productMutations.updateProduct);
        fetchProducts();
      } else {
        console.error('Failed to update product:', response.errorMessage);
      }
    } catch (error) {
      console.error('Error updating product:', error);
    }
  };


  //============================

  return (
    <ManagerViewContext.Provider value={{ handleDataChange: handleDataChange }}>
      <CRow>
        <CButton
          color="primary"
          className="mb-3"
          onClick={handleAddProduct}
        >
          Add New Product
        </CButton>
        <CButton
          color="primary"
          className="mb-3"
          onClick={handleDeleteProduct}
        >
          Delete Product
        </CButton>
              <CButton
          color="primary"
          className="mb-3"
          onClick={handleUpdateProduct}
        >
          Update Product
        </CButton>
        <CCol xs={12}>
          {isLoading ? (
            <SurveyTalkLoading />
          ) : (
            <div
              id="Manager-table"
            >
              <AgGridReact
                columnDefs={state?.columnDefs}
                rowData={state?.rowData}
                defaultColDef={defaultColDef}
                rowHeight={70}
                headerHeight={40}
                pagination={true}
                paginationPageSize={10}
                paginationPageSizeSelector={[10, 20, 50, 100]}
                domLayout='autoHeight'
              />
            </div>)}
        </CCol>
      </CRow>

    </ManagerViewContext.Provider>
  )
}

export default ManagerView;

export const GET_ALL_PRODUCTS = `
query GetAllProducts($minPrice: Decimal, $maxPrice: Decimal, $sortBy: [ProductSortInput!]) {
  _dbQueries {
    _productQueries {
      allProducts(
        where: {
          and: [
            { price: { gte: $minPrice } }
            { price: { lte: $maxPrice } }
          ]
        }
        order: $sortBy
      ) {
        productId
        productName
        price
        stockQuantity
        createdAt

      }
    }
  }
}
`;
export const ADD_PRODUCT = `
  mutation AddProduct(
    $productName: String!
    $categoryId: Int
    $price: Decimal!
    $stockQuantity: Int
  ) {
    _dbMutations {
      _productMutations {
        addProduct(
          productName: $productName
          categoryId: $categoryId
          price: $price
          stockQuantity: $stockQuantity
        ) {
          productId
          productName
          price
          stockQuantity
          createdAt
          category {
            categoryId
            categoryName
            description
            createdAt
          }
        }
      }
    }
  }
`;
export const DELETE_PRODUCT = `
  mutation DeleteProduct(
  $productId: Int!
) {
  _dbMutations {
    _productMutations {
      deleteProduct(
        productId: $productId
      ) {
        rootElement
      }
    }
  }
}
`;
export const UPDATE_PRODUCT = `
  mutation UpdateProduct(
    $productId: Int!
    $productName: String!
    $stockQuantity : Int
    $price: Decimal!
  ) {
    _dbMutations {
      _productMutations {
        updateProduct(
          productId: $productId
          productName: $productName
          price: $price
          stockQuantity: $stockQuantity
        ) {
          productId
          productName
          price
          stockQuantity
          createdAt
          category {
            categoryId
            categoryName
            description
            createdAt
          }
        }
      }
    }
  }
`;
export const getAllProducts = async (filters?: any) => {
  return await callGraphQLQuery({
    client: publicApolloClient,
    query: GET_ALL_PRODUCTS,
    variables: {
      minPrice: filters?.minPrice,
      maxPrice: filters?.maxPrice,
      sortBy: filters?.sortBy,
    }
  }, "Lấy danh sách sản phẩm");
};
export const addProduct = async (product: any) => {
  return await callGraphQLMutation({
    client: publicApolloClient,
    mutation: ADD_PRODUCT,
    variables: product
  }, "Thêm sản phẩm mới");
};
export const deleteProduct = async (productId: any) => {
  return await callGraphQLMutation({
    client: publicApolloClient,
    mutation: DELETE_PRODUCT,
    variables: { productId }
  }, "Xóa sản phẩm");
};
export const updateProduct = async (product: any) => {
  return await callGraphQLMutation({
    client: publicApolloClient,
    mutation: UPDATE_PRODUCT,
    variables: { productId: product.productId, productName: product.productName, stockQuantity: product.stockQuantity, price: product.price }
  }, "Cập nhật sản phẩm");
};