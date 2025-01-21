<template>
  <div class="login-form">
    <h2>Giriş Yap</h2>
    <DxForm
        :formData="loginData"
        :colCount="1"
        :showColonAfterLabel="true"
        :labelLocation="'top'"
    >
      <DxItem dataField="Kullanıcı Adı" [label]="{ text: 'Kullanıcı Adı' }">
        <DxTextBox
            v-model="loginData.username"
            placeholder="Kullanıcı adınızı girin"
            :showClearButton="true"
        />
      </DxItem>
      <DxItem dataField="Parola" [label]="{ text: 'Parola' }">
        <DxTextBox
            v-model="loginData.password"
            mode="password"
            placeholder="Parolanızı girin"
            :showClearButton="true"
        />
      </DxItem>
      <DxItem>
        <DxButton
            text="Giriş Yap"
            type="success"
            @click="handleLogin"
        />
      </DxItem>
    </DxForm>
  </div>
</template>

<script>
import { reactive } from 'vue';
import { DxForm, DxItem } from 'devextreme-vue/form';
import DxTextBox from 'devextreme-vue/text-box';
import DxButton from 'devextreme-vue/button';
import axios from 'axios';

export default {
  components: {
    DxForm,
    DxItem,
    DxTextBox,
    DxButton,
  },
  setup() {
    const loginData = reactive({
      username: '',
      password: '',
    });

    const handleLogin = async () => {
      if (loginData.username && loginData.password) {
        try {
          const response = await axios.post('https://localhost:7067/api/login', {
            username: loginData.username,
            password: loginData.password,
          });
          if (response.data.success) {
            // Başarılı giriş sonrası kullanıcıyı yönlendirme
            window.location.href = "http://localhost:8080/dashboard";
          } else {
            alert('Hatalı kullanıcı adı veya parola.');
          }
        } catch (error) {
          alert('Bir hata oluştu. Lütfen tekrar deneyin.');
        }
      } else {
        alert('Lütfen tüm alanları doldurun.');
      }
    };

    return {
      loginData,
      handleLogin,
    };
  },
};
</script>

<style scoped>
.login-form {
  max-width: 400px;
  margin: 100px auto;
  padding: 20px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background-color: #f9f9f9;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}
h2 {
  text-align: center;
  margin-bottom: 20px;
}
</style>
