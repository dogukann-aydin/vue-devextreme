<template>
  <DxDrawer
      :opened="isDrawerOpen"
      position="left"
      :shading="false"
      :closeOnOutsideClick="false"
      :minSize="50"
      :maxSize="250"
      :revealMode="'expand'">

    <!-- Navbar İçeriği -->
    <template #template>
      <DxList
          :items="menuItems"
          :selectionMode="'single'"
          @itemClick="onMenuItemClick"
      />
    </template>

    <!-- İçerik Alanı -->
    <div class="content">
      <!-- Sekmeler Alanı -->
      <DxToolbar>
        <template #content>
          <div class="header">
            <DxButton
                text="Yeni Stok Girişi"
                icon="add"
                type="success"
                @click="onNewStokClick"
                class="action-button"
            />
            <DxButton
                text="Stok Düzenle"
                icon="edit"
                type="default"
                @click="onEditSelectedProduct"
                class="action-button"
            />
            <DxButton
                text="Stok Sil"
                icon="trash"
                type="danger"
                @click="onDeleteSelectedProduct"
                class="action-button"
            />
          </div>
        </template>
      </DxToolbar>

      <!-- Sekmeler -->
      <div class="tab-bar">
        <div
            v-for="(tab, index) in openedTabs"
            :key="tab.id"
            class="tab"
            :class="{'active': activeTabIndex === index}"
            @click="setActiveTab(index)">
          <span>{{ tab.text }}</span>
          <DxButton
              icon="close"
              @click="closeTab(index)"
              class="close-button"
          />
        </div>
      </div>

      <!-- Dinamik İçerik Alanı -->
      <div class="page-content">
        <h1>{{ currentPageTitle }}</h1>
        <p>{{ currentPageContent }}</p>

        <!-- Stok Listesi DataGrid -->
        <DxDataGrid
            v-if="currentPageTitle === 'Stok Listesi'"
            :dataSource="dataSource"
            :columns="columns"
            :selectionMode="'single'"
            :searchPanel="{ visible: true, highlightCaseSensitive: true }"
            :editing="{
            mode: 'popup',
            allowAdding: true,
            allowUpdating: true,
            allowDeleting: true
          }"
            @onRowInserted="onRowInserted"
            @onRowUpdated="onRowUpdated"
            @onRowRemoved="onRowRemoved"
            v-model:selectedRowKeys="selectedRowKeys"
        />
      </div>
    </div>
  </DxDrawer>
</template>

<script>
import { ref } from "vue";
import { DxDrawer } from "devextreme-vue/drawer";
import { DxList } from "devextreme-vue/list";
import { DxToolbar } from "devextreme-vue/toolbar";
import { DxButton } from "devextreme-vue/button";
import { DxDataGrid } from "devextreme-vue/data-grid";

export default {
  name: "IndexPage",
  components: {
    DxDrawer,
    DxList,
    DxToolbar,
    DxButton,
    DxDataGrid,
  },
  setup() {
    const isDrawerOpen = ref(true);
    const menuItems = ref([
      { text: "Stok Listesi", id: 1 },
      { text: "Hakkımızda", id: 2 },
      { text: "Hizmetler", id: 3 },
      { text: "İletişim", id: 4 },
    ]);
    const openedTabs = ref([]);
    const activeTabIndex = ref(null);
    const currentPageTitle = ref("Stok Listesi");
    const currentPageContent = ref("Stok Listesi içeriği burada yer alacak.");
    const dataSource = ref([
      { name: "Ürün A", price: 100, unit: "gr", number: 23 },
      { name: "Ürün B", price: 150, unit: "kg", number: 105 },
      { name: "Ürün C", price: 200, unit: "gr", number: 80 },
    ]);
    const columns = ref([
      { dataField: "name", caption: "Ürün Adı" },
      { dataField: "price", caption: "Fiyat" },
      { dataField: "unit", caption: "Birim" },
      { dataField: "number", caption: "Adet" },
    ]);
    const selectedRowKeys = ref([]);

    const toggleDrawer = () => {
      isDrawerOpen.value = !isDrawerOpen.value;
    };

    const onMenuItemClick = (e) => {
      const selectedTab = e.itemData;
      const tabExists = openedTabs.value.some((tab) => tab.id === selectedTab.id);

      if (!tabExists) {
        openedTabs.value.push({
          ...selectedTab,
        });
      }

      activeTabIndex.value = openedTabs.value.length - 1;

      currentPageTitle.value = selectedTab.text;
      currentPageContent.value = `${selectedTab.text} içeriği burada yer alacak.`;
    };

    const closeTab = (index) => {
      openedTabs.value.splice(index, 1);

      if (activeTabIndex.value === index) {
        if (openedTabs.value.length > 0) {
          activeTabIndex.value = 0;
          const activeTab = openedTabs.value[0];
          currentPageTitle.value = activeTab.text;
          currentPageContent.value = `${activeTab.text} içeriği burada yer alacak.`;
        } else {
          activeTabIndex.value = null;
          currentPageTitle.value = '';
          currentPageContent.value = '';
        }
      }
    };

    const setActiveTab = (index) => {
      if (openedTabs.value[index]) {
        activeTabIndex.value = index;
        const activeTab = openedTabs.value[index];
        currentPageTitle.value = activeTab.text;
        currentPageContent.value = `${activeTab.text} içeriği burada yer alacak.`;
      } else {
        currentPageTitle.value = '';
        currentPageContent.value = '';
      }
    };

    const onNewStokClick = () => {
      const newStok = { name: "Yeni Ürün", price: 0, unit: "", number: 0 };
      dataSource.value.push(newStok);
      alert("Yeni stok başarıyla eklendi!");
    };

    const onEditSelectedProduct = () => {
      if (selectedRowKeys.value.length > 0) {
        const selectedProduct = dataSource.value.find(product => product.name === selectedRowKeys.value[0]);
        selectedProduct.name = "Düzenlenmiş Ürün";
        selectedProduct.price = 999;
        alert("Stok başarıyla düzenlendi!");
      } else {
        alert("Lütfen düzenlemek için bir ürün seçin.");
      }
    };

    const onDeleteSelectedProduct = () => {
      if (selectedRowKeys.value.length > 0) {
        const selectedProduct = dataSource.value.find(product => product.name === selectedRowKeys.value[0]);
        dataSource.value = dataSource.value.filter(product => product !== selectedProduct);
        alert("Stok başarıyla silindi!");
      } else {
        alert("Lütfen silmek için bir ürün seçin.");
      }
    };

    return {
      isDrawerOpen,
      menuItems,
      openedTabs,
      activeTabIndex,
      currentPageTitle,
      currentPageContent,
      dataSource,
      columns,
      selectedRowKeys,
      toggleDrawer,
      onMenuItemClick,
      closeTab,
      setActiveTab,
      onNewStokClick,
      onEditSelectedProduct,
      onDeleteSelectedProduct,
    };
  },
};
</script>

<style scoped>
.content {
  padding: 20px;
  background-color: #f9f9f9;
}

.header {
  margin-bottom: 10px;
  text-align: left;
}

.action-button {
  margin-right: 5px;
}

.tab-bar {
  display: flex;
  padding: 10px 0;
  background-color: #f1f1f1;
  border-bottom: 1px solid #ddd;
}

.tab {
  display: flex;
  align-items: center;
  padding: 5px 15px;
  background-color: #e0e0e0;
  margin-right: 10px;
  border-radius: 4px;
  cursor: pointer;
}

.tab.active {
  background-color: #3f51b5;
  color: white;
}

.close-button {
  margin-left: 10px;
}

.page-content {
  margin-top: 20px;
}
</style>
