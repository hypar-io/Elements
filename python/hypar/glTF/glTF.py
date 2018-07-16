import sys
import json
from enum import Enum
import os
from struct import *
import base64


# https://github.com/KhronosGroup/glTF-Sample-Models/blob/master/2.0/Box/glTF/Box.gltf
class glTFEncoder(json.JSONEncoder):
    """A custom encoder for our glTF class. 
    An instance of glTF is comprised of a number of class 
    instances which, using JSONEncoder, are not serializable
    by default. This encoder simply provides the dictionary
    representation of the object, removing any values that 
    are None, as these would serialize to null in the glTF,
    and that is not allowed.
    """

    def default(self, obj):
        if isinstance(obj, componentType):
            return obj.value
        if isinstance(obj, componentName):
            return obj.value
        if isinstance(obj, accessorType):
            return obj.value
        if isinstance(obj, pbrMetallicRoughness):
            return to_dict(obj)
        if isinstance(obj, primitive):
            return to_dict(obj)
        if isinstance(obj,attributes):
            return to_dict(obj)
        if isinstance(obj, primitiveMode):
            return obj.value
        if isinstance(obj, alphaMode):
            return obj.value
        if isinstance(obj, glTF):
            gltf_dict = {}
            gltf_dict['asset'] = obj.assett.__dict__
            gltf_dict['scene'] = obj.scene
            gltf_dict['scenes'] = [to_dict(ob) for ob in obj.scenes]
            gltf_dict['nodes'] = [to_dict(ob) for ob in obj.nodes]
            gltf_dict['meshes'] = [to_dict(ob) for ob in obj.meshes]
            gltf_dict['accessors'] = [to_dict(ob) for ob in obj.accessors]
            gltf_dict['buffers'] = [to_dict(ob) for ob in obj.buffers]
            gltf_dict['bufferViews'] = [to_dict(ob) for ob in obj.bufferViews]
            gltf_dict['materials'] = [to_dict(ob) for ob in obj.materials]
            gltf_dict['extensionsUsed'] = obj.extensionsUsed
            return gltf_dict


def to_dict(obj):
    d = obj.__dict__
    return del_none(d)


def del_none(d):
    """
    Delete keys with the value ``None`` in a dictionary, recursively.
    This alters the input so you may wish to ``copy`` the dict first.
    """
    for key, value in list(d.items()):
        if value is None:
            del d[key]
        elif isinstance(value, dict):
            del_none(value)
    return d  # For convenience


class alphaMode(Enum):
    BLEND = "BLEND"
    MASK = "MASK"
    OPAQUE = "OPAQUE"


class primitiveMode(Enum):
    POINTS = 0
    LINES = 1
    LINE_LOOP = 2
    LINE_STRIP = 3
    TRIANGLES = 4
    TRIANGLE_STRIP = 5
    TRIANGLE_FAN = 6


class componentType(Enum):
    BYTE = 5120
    UNSIGNED_BYTE = 5121
    SHORT = 5122
    UNSIGNED_SHORT = 5123
    UNSIGNED_INT = 5125
    FLOAT = 5126


class componentName(Enum):
    POSITION = 'POSITION'
    NORMAL = 'NORMAL'
    TANGENT = 'TANGENT'
    TEXCOORD_0 = 'TEXCOORD_0'
    TEXCOORD_1 = 'TEXCOORD_1'
    COLOR_0 = 'COLOR_0'
    JOINTS_0 = 'JOINTS_0'
    WEIGHTS_0 = 'WEIGHTS_0'


class accessorType(Enum):
    SCALAR = 'SCALAR'
    VEC2 = 'VEC2'
    VEC3 = 'VEC3'
    VEC4 = 'VEC4'
    MAT2 = 'MAT2'
    MAT3 = 'MAT3'
    MAT4 = 'MAT4'


class glTF():
    """A class which represents a glTF file.
    """

    def __init__(self):
        # Default assett
        self.assett = asset()
        
        # Add a root node.
        root_node = node([],[1.0,0.0,0.0,0.0,
                            0.0,0.0,-1.0,0.0,
                            0.0,1.0,0.0,0.0,
                            0.0,0.0,0.0,1.0])
        self.nodes = [root_node]

        # Add a default scene referencing the 
        # root node.
        self.scene = 0
        s = scene([0])
        self.scenes = [s]

        self.meshes = []
        self.accessors = []

        self.materials = []

        self.buffers = []
        self.bufferViews = []

        self.vertices = []
        self.normals = []
        self.indices = []

        self.buffer = []

        self.extensionsUsed = ['KHR_materials_pbrSpecularGlossiness']

    def save(self, path):
        """Save a glTF file to disk.
        The file will be saved as {path}
        The associated buffer will be saved as {filename}.bin
        
        Arguments:
            path {string} -- The path to the file on disk.
        """

        b = bytes(self.buffer)
        base = os.path.basename(path)
        bin_name = str.format('{}.bin', os.path.splitext(base)[0])
        bin_path = os.path.join(os.path.dirname(path), bin_name)
        bf = open(bin_path, 'wb')
        bf.write(b)
        bf.close()
        
        buff = buffer(len(b),bin_name)
        self.buffers.append(buff)

        f = open(path,'w')
        f.write(json.dumps(self, indent=4, cls=glTFEncoder, skipkeys=True))
        f.close()

    def create_glb(self):
        buff = buffer(len(self.buffer))
        self.buffers.append(buff)

        jsonb = bytearray(json.dumps(self, cls=glTFEncoder, skipkeys=True).encode('utf-8'))
        bin = bytearray(self.buffer)

        # Pad to 4 byte boundary with spaces.
        while len(jsonb) % 4 != 0:
            jsonb.extend(" ".encode('utf-8'))
   
        # Pad to 4 byte boundary with zeros
        while len(bin) % 4 != 0:
            bin.append(0)
  
        # Create a buffer
        glb = bytearray()

        # Write the header
        glb.extend("glTF".encode('ascii'))
        glb.extend(pack('I',2))
        glb.extend(pack('I',12 + 8 + len(jsonb) + 8 + len(bin)))

        # Chunk 1 - JSON
        glb.extend(pack('I',len(jsonb)))
        glb.extend("JSON".encode('ascii'))
        glb.extend(jsonb)

        # Chunk 2 - BIN
        glb.extend(pack('I',len(bin)))
        glb.extend("BIN".encode('ascii'))
        # Pad with one empty byte to get proper alignment.
        glb.append(0)
        glb.extend(bin)
        return glb

    def save_base64(self):
        """Save a glTF as a base64 encoded string.
        
        Returns:
            string -- The base64 encoded string representing the glTF.
        """

        glb = self.create_glb()
        ENCODING = 'utf-8'
        return base64.b64encode(glb).decode(ENCODING)

    def save_glb(self, path):
        """Save a glb file to disk
        The file will be saved as {path}
        
        Arguments:
            path {string} -- The path to the file on disk.
        """
        glb = self.create_glb()
        glbf = open(path, 'wb')
        glbf.write(glb)
        glbf.close()

    def add_material(self, red, green, blue, alpha, metallic_factor, name):
        """Add a material to glTF.
        
        Arguments:
            material {material} -- The material to add.

        Returns:
            integer -- The index of the newly added material.
        """
        m = material(red, green, blue, alpha, metallic_factor, name)
        self.materials.append(m)
        return len(self.materials)-1

    def add_triangle_mesh(self, vertices, normals, indices, material_id, parent_index = None):
        """Add a triangle mesh to the scene.
        
        Arguments:
            vertices {list} -- A list of floats (x,y,z,x,y,z...) representing the vertices of the mesh.
            normals {list} -- A list of floats (nx,ny,nz,nx,ny,nz...) representing the normals of the mesh.
            indices {list} -- A list of integers (0,1,2,2,3,4...) representing the vertex indices of the mesh.
        
        Keyword Arguments:
            parent_index {integer} -- The index of the parent node. (default: {None})

        Returns:
            integer -- The index of the newly added mesh.
        """

        v_max_x = sys.float_info.min
        v_max_y = sys.float_info.min
        v_max_z = sys.float_info.min
        v_min_x = sys.float_info.max
        v_min_y = sys.float_info.max
        v_min_z = sys.float_info.max
        for i in range(0, len(vertices),3):
            if vertices[i] > v_max_x:
                v_max_x = vertices[i]
            if vertices[i] < v_min_x:
                v_min_x = vertices[i]
            if vertices[i+1] > v_max_y:
                v_max_y = vertices[i+1]
            if vertices[i+1] < v_min_y:
                v_min_y = vertices[i+1]
            if vertices[i+2] > v_max_z:
                v_max_z = vertices[i+2]
            if vertices[i+2] < v_min_z:
                v_min_z = vertices[i+2]

        n_max_x = sys.float_info.min
        n_max_y = sys.float_info.min
        n_max_z = sys.float_info.min
        n_min_x = sys.float_info.max
        n_min_y = sys.float_info.max
        n_min_z = sys.float_info.max
        for i in range(0, len(normals),3):
            if normals[i] > n_max_x:
                n_max_x = normals[i]
            if normals[i] < n_min_x:
                n_min_x = normals[i]
            if normals[i+1] > n_max_y:
                n_max_y = normals[i+1]
            if normals[i+1] < n_min_y:
                n_min_y = normals[i+1]
            if normals[i+2] > n_max_z:
                n_max_z = normals[i+2]
            if normals[i+2] < n_min_z:
                n_min_z = normals[i+2]

        i_max = max(indices)
        i_min = min(indices)

        #self.vertices.extend(vertices)
        #self.normals.extend(normals)
        #self.indices.extend(indices)
        
        # Add buffer views
        # A buffer view for vertices.
        vert_buff = self.add_buffer_view(0, len(self.buffer), len(vertices) * 4)
        # A buffer view for normals.
        norm_buff = self.add_buffer_view(0, len(self.buffer) + len(vertices) * 4, len(normals) * 4)
        # The buffer view for indices is offset by the total bytes for vertices and normals
        index_buff = self.add_buffer_view(0, len(self.buffer) + len(vertices) * 4 + len(normals) * 4, len(indices) * 2)

        '''
        Pack data into the buffer.
        The buffer is organized as vertices|normals|indices.
        If you don't place indices at the end, and try to use
        an offset for vertices and normals of a multiple of 2 bytes (ushort),
        glTF will bark at you that the offset needs to be a multiple of
        the component type. If the buffer's length after packing is not
        a multiple of four, we pad it with two bytes.
        '''
        self.buffer.extend(pack("%sf"%len(vertices), *vertices))
        self.buffer.extend(pack("%sf"%len(normals), *normals))
        self.buffer.extend(pack("%sH"%len(indices), *indices))
        if(len(self.buffer) % 4 != 0):
            self.buffer.extend(pack("H",0))

        # Add accesors
        # The vertex accessor has a length of vertices/3 and max and min values set by the bounds of the geometry.
        vert_access = self.add_accessor(vert_buff, 0, componentType.FLOAT, int(len(vertices)/3), [v_min_x,v_min_y,v_min_z], [v_max_x,v_max_y,v_max_z], accessorType.VEC3)
        # The normal accessor has an offset of the byte length of vertices, a length of normals/3, and max and min values
        # set by the bounds of the normals
        norm_access = self.add_accessor(norm_buff, 0, componentType.FLOAT, int(len(normals)/3), [n_min_x,n_min_y,n_min_z], [n_max_x,n_max_y,n_max_z], accessorType.VEC3)
        # The index accessor has a length equal to the indices array, and a max of the largest index
        index_access = self.add_accessor(index_buff, 0, componentType.UNSIGNED_SHORT, int(len(indices)), [i_min], [i_max], accessorType.SCALAR)

        attr = attributes(vert_access, norm_access)
        prim = primitive(attr, index_access, material_id, primitiveMode.TRIANGLES)
        m = mesh([prim])

        self.meshes.append(m)

        # Add a new mesh node
        mesh_node = node(meshId=len(self.meshes)-1)
        self.add_node(mesh_node, 0)

        return len(self.meshes)-1
    
    def add_node(self, node, parent_node_id):
        """Add a node.
        
        Arguments:
            node {node} -- The node to add.
            parent_node_id {integer} -- The index of the parent node.
        
        Returns:
            [type] -- [description]
        """

        self.nodes.append(node)
        node_id = len(self.nodes)-1
        self.nodes[parent_node_id].children.append(node_id)
        return node_id

    def add_accessor(self, bufferViewId, byteOffset, componentType, count, minArr, maxArr, accessorType):
        """Add an accessor to a buffer view.
        
        Arguments:
            bufferViewId {integer} -- The index of the buffer view.
            byteOffset {integer} -- The offset, in bytes, from the beginning of the buffer view.
            componentType {componentType} -- The component type.
            count {integer} -- The number of entities represented by the buffer view.
            maxArr {list} -- The maximum values for each component of an entity in the buffer view.
            minArr {list} -- The minimum values for each component of an entity in the buffer view.
            accessorType {accesorType} -- The accessor type. Ex: accessorType.VEC3
        
        Returns:
            integer -- The index of the newly added accessor.
        """
        a = accessor(bufferViewId, byteOffset, componentType, count, minArr, maxArr, accessorType)
        self.accessors.append(a)
        return len(self.accessors)-1

    def add_buffer_view(self, bufferIndex, byteOffset, byteLength, byteStride=None):
        """Add a buffer view.
        
        Arguments:
            bufferIndex {integer} -- The index of the buffer which this view uses.
            byteOffset {integer} -- The offset in bytes from the start of the buffer.
            byteLength {integer} -- The length of the buffer view, in bytes.
        
        Returns:
            integer -- The index of the newly added buffer view.
        """

        buffView = bufferView(bufferIndex, byteOffset, byteLength, None, byteStride)
        self.bufferViews.append(buffView)
        return len(self.bufferViews)-1


class asset:
    def __init__(self):
        self.generator = "hypar-gltf"
        self.version = "2.0"


class scene:
    def __init__(self, nodes):
        self.nodes = nodes


class node:
    def __init__(self, childIds=None, matrix=None, meshId=None):
        self.children = childIds
        self.matrix = matrix
        self.mesh = meshId


class mesh:
    def __init__(self, primitives=None, name="Mesh"):
        self.primitives = primitives
        self.name = name


class accessor:
    def __init__(self, bufferViewId, byteOffset, componentType, count, minArr, maxArr, accessorType):
        self.bufferView = bufferViewId
        self.byteOffset = byteOffset
        self.componentType = componentType
        self.count = count
        self.max = maxArr
        self.min = minArr
        self.type = accessorType


class material:
    def __init__(self, red, green, blue, alpha, metallic_factor, name):
        pbr = pbrMetallicRoughness([red,green,blue,alpha],metallic_factor)
        self.pbrMetallicRoughness = pbr
        self.name = name
        if alpha < 1.0:
            self.alphaMode = alphaMode.BLEND
        else:
            self.alphaMode = alphaMode.OPAQUE
        self.extensions = {
            'KHR_materials_pbrSpecularGlossiness': {
                'diffuseFactor': [red, green, blue, alpha],
                'specularFactor': [1.0,1.0,1.0],
                'glossinessFactor': 0.5
            }
        }


class pbrMetallicRoughness:
    def __init__(self, baseColorFactor=[1.0,0.0,0.0,1.0], metallicFactor=1.0):
        self.baseColorFactor = baseColorFactor 
        self.metallicFactor = metallicFactor


class bufferView:
    def __init__(self, bufferId, byteOffset, byteLength, target, byteStride=None):
        self.buffer = bufferId
        self.byteOffset = byteOffset
        self.byteLength = byteLength
        self.byteStride = byteStride
        self.target = target


class buffer:
    def __init__(self, byteLength, uri = None):
        self.byteLength = byteLength
        self.uri = uri


class primitive:
    def __init__(self, attributes, indicesId, materialId, mode):
        self.attributes = attributes
        self.indices = indicesId
        self.material = materialId
        self.mode = mode


class attributes:
    def __init__(self, positionAccessorId, normaAccessorlId):
        self.NORMAL = normaAccessorlId
        self.POSITION = positionAccessorId
