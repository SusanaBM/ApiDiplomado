pipeline {
    agent any

    environment {
        DOTNET_ROOT = "${HOME}/.dotnet"
        PATH = "${HOME}/.dotnet:${HOME}/.dotnet/tools:${env.PATH}"
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "1"
        REGISTRY = "demoapiregistry.azurecr.io"
        IMAGE_NAME = "demo-api"
        IMAGE_TAG = GIT_COMMIT.take(7)
        NAMESPACE = "demo-api"
        CHART_REPO = "https://susanabm.github.io/demo-api-helm/"
        PRINCIPAL_DIR = "demo-api-helm"
        K8SREPO = "github.com/3sneider/k8sRepository.git"
        GITHUB_CREDS = "github-creds-su"
        ACR_CREDS = "acr-creds"

        CHART_DIR  = 'demo-chart'
        DOCS_DIR   = 'docs'
        CHART_NAME = 'demo-chart'
        CHART_PKG_URL = 'https://susanabm.github.io/demo-api-helm/'
        VERSION    = '' // se calculará dinámicamente
        GITOPS_REPO = 'github.com/SusanaBM/demo-api-helm.git'
        GITOPS_DIR = 'argocd-apps'
    }

    stages {      
        stage('Checkout') {
            steps {
                checkout scm
            }
        }      

        stage('Install .NET') {
            steps {
                sh '''
                    curl -sSL -o install-dotnet.sh https://dot.net/v1/dotnet-install.sh
                    chmod +x install-dotnet.sh
                    ./install-dotnet.sh --channel 9.0 --install-dir $DOTNET_ROOT
                '''
            }
        }

        stage('Check .NET') {
            steps {
                sh 'dotnet --info'
            }
        }

        stage('Crear Tool Manifest') {
            steps {
                sh 'dotnet new tool-manifest --force'
                echo '✅ Tool manifest creado exitosamente.'
            }
        }

        stage('Instalar EF Core Tools') {
            steps {
                sh 'dotnet tool install dotnet-ef'
                echo '✅ EF Core Tools instalados exitosamente.'
            }
        }

        stage('Restaurar Dependencias') {
            steps {
                sh 'dotnet restore'
                sh 'dotnet tool restore'
                echo '✅ Dependencias restauradas exitosamente.'
            }
        }

        stage('Compilar') {
            steps {
                sh 'dotnet build --configuration Release --no-restore'
                echo '✅ Compilación exitosa.'
            }
        }

        stage('Ejecutar Pruebas') {
            steps {
                sh 'dotnet test --configuration Release --no-build --verbosity normal'
                echo '✅ Pruebas ejecutadas exitosamente.'
            }
        }

        stage('Publicar Artefactos') {
            steps {
                sh 'dotnet publish DemoApi.csproj -c Release -o out'
                archiveArtifacts artifacts: 'out/**/*', fingerprint: true
                echo '✅ Artefactos publicados exitosamente.'
            }
        }        

        stage('Login to ACR') {
            steps {
                withCredentials([usernamePassword(credentialsId: "${ACR_CREDS}",
                                                 usernameVariable: 'AZ_USER',
                                                 passwordVariable: 'AZ_PASS')]) {
                    sh """
                        echo \$AZ_PASS | docker login $REGISTRY -u \$AZ_USER --password-stdin
                    """
                }
            }
        }

        stage('Build Docker Image') {
            steps {
                sh """
                    docker build -t $REGISTRY/$IMAGE_NAME:$IMAGE_TAG --target final .
                """
            }
        }

        stage('Build Migration Docker Image') {
            steps {
                sh """
                    docker build -f Dockerfile -t $REGISTRY/$IMAGE_NAME:migration --target migration .
                """
            }
        }

        stage('Push Docker Image') {
            steps {
                sh """
                    docker push $REGISTRY/$IMAGE_NAME:$IMAGE_TAG
                """
            }
        }

        stage('Push Migration Docker Image') {
            steps {
                sh """
                    docker push $REGISTRY/$IMAGE_NAME:migration
                """
            }
        }

        stage('Clone Chart-GitOps repo') {
            steps {
                dir("demo-api-helm") {
                    deleteDir()
                }
                withCredentials([usernamePassword(credentialsId: "${GITHUB_CREDS}", usernameVariable: 'GIT_USER', passwordVariable: 'GIT_TOKEN')]) {
                    
                    sh """
                        git clone https://${GITOPS_REPO}

                        ls -la                        
                    """
                }
               
            }
        }

        stage('Package Helm Chart') {
            steps {
                script {
                    // Obtener versión desde Chart.yaml
                    VERSION = sh(
                        script: "grep '^version:' ${PRINCIPAL_DIR}/${CHART_DIR}/Chart.yaml | awk '{print \$2}'",
                        returnStdout: true
                    ).trim()
                    
                    echo "Chart version: ${VERSION}"
                    sh """
                      cd ${PRINCIPAL_DIR}
                      helm lint ${CHART_DIR}
                      helm dependency update ${CHART_DIR}
                      helm package ${CHART_DIR} -d ${DOCS_DIR}
                      helm repo index ${DOCS_DIR} --url ${CHART_PKG_URL} --merge ${DOCS_DIR}/index.yaml || true
                    """
                }
            }
        }

        stage('Update GitOps repo') {
            steps {
                withCredentials([usernamePassword(credentialsId: "${GITHUB_CREDS}", usernameVariable: 'GIT_USER', passwordVariable: 'GIT_TOKEN')]) {
                    sh """

                        cd ${PRINCIPAL_DIR}

                        ls -la

                        sed -i 's/targetRevision:.*/targetRevision: ${VERSION}/' ${GITOPS_DIR}/demo-api.yaml

                        git config user.email "action@github.com"
                        git config user.name "Github Action"
                        git add ${DOCS_DIR} ${GITOPS_DIR}/demo-api.yaml
                        git commit -m "Release ${CHART_NAME} version ${VERSION}" || echo "No hay cambios para commitear"
                        git remote set-url origin https://${GIT_USER}:${GIT_TOKEN}@${GITOPS_REPO}
                        git push origin main
                    """
                }               
            }
        }
    }

    post {
        success {
            echo '✅ Pipeline completado con éxito.'
        }
        failure {
            echo '❌ Pipeline falló.'
        }
    }
}
